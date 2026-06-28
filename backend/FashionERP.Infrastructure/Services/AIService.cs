using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.AI;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class AIService : IAIService
    {
        private readonly AppDbContext _db;
        private readonly IAIServiceClient _aiClient;

        public AIService(AppDbContext db, IAIServiceClient aiClient)
        {
            _db = db;
            _aiClient = aiClient;
        }

        // ===================== CHATBOT =====================

        public async Task<ChatbotResponseDto> ChatAsync(ChatbotRequestDto request, Guid? userId)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                throw new AppException("Nội dung tin nhắn không được để trống", 400);

            var stopwatch = Stopwatch.StartNew();
            // FIXED: khai báo có giá trị mặc định null! để tránh CS0165 "use of unassigned local
            // variable" khi exception xảy ra trong try trước khi result được gán - finally vẫn
            // chạy và đọc result, nên compiler yêu cầu phải definite-assigned từ đầu.
            ChatbotResponseDto result = null!;
            bool isSuccess = true;
            string? errorMessage = null;

            try
            {
                // Lấy tối đa 20 sản phẩm đang active còn hàng để AI có context tư vấn
                // FIXED: p.Status là enum ProductStatus, không phải string -> so sánh với ProductStatus.Active
                var products = await _db.Products
                    .Where(p => p.Status == ProductStatus.Active)
                    .Include(p => p.Category)
                    .Include(p => p.Variants)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(20)
                    .Select(p => new AIProductContextDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CategoryName = p.Category != null ? p.Category.Name : null,
                        BasePrice = p.BasePrice,
                        TotalStock = p.Variants
                            .Where(v => v.IsActive)
                            .Sum(v => v.Inventory != null ? v.Inventory.Quantity : 0)
                    })
                    .ToListAsync();

                // Lấy các khuyến mãi đang active
                var now = DateTime.UtcNow;
                var promotions = await _db.Promotions
                    .Where(pr => pr.IsActive && pr.StartDate <= now && pr.EndDate >= now)
                    .Select(pr => new AIPromotionContextDto
                    {
                        Code = pr.Code,
                        Name = pr.Name,
                        // FIXED: pr.Type là enum PromotionType, AIPromotionContextDto.Type là string -> convert
                        Type = pr.Type.ToString(),
                        DiscountValue = pr.DiscountValue
                    })
                    .ToListAsync();

                var proxyRequest = new AIChatbotProxyRequest
                {
                    Message = request.Message.Trim(),
                    History = request.History,
                    ProductContext = products,
                    PromotionContext = promotions
                };

                result = await _aiClient.ChatAsync(proxyRequest);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                errorMessage = ex.Message;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                await SafeLogAsync(AIFeature.Chatbot, userId,
                    inputData: JsonSerializer.Serialize(new { request.Message }),
                    outputData: isSuccess ? JsonSerializer.Serialize(result) : null,
                    durationMs: (int)stopwatch.ElapsedMilliseconds,
                    isSuccess: isSuccess,
                    errorMessage: errorMessage);
            }

            return result;
        }

        // ===================== SIZE RECOMMEND =====================

        public async Task<SizeRecommendResponseDto> RecommendSizeAsync(SizeRecommendRequestDto request, Guid? userId)
        {
            var stopwatch = Stopwatch.StartNew();
            SizeRecommendResponseDto result = null!; // FIXED: tránh CS0165
            bool isSuccess = true;
            string? errorMessage = null;

            try
            {
                // FIXED: SizeChart.ProductType / Gender là enum (SizeChartProductType / SizeChartGender),
                // còn request.ProductType / request.Gender là string nhập từ FE -> phải parse trước khi so sánh.
                if (!Enum.TryParse<SizeChartProductType>(request.ProductType, true, out var productType))
                    throw new AppException(
                        $"Loại sản phẩm '{request.ProductType}' không hợp lệ (phải là: " +
                        $"{string.Join(", ", Enum.GetNames(typeof(SizeChartProductType)))})", 400);

                if (!Enum.TryParse<SizeChartGender>(request.Gender, true, out var sizeGender))
                    throw new AppException(
                        $"Giới tính '{request.Gender}' không hợp lệ (phải là: " +
                        $"{string.Join(", ", Enum.GetNames(typeof(SizeChartGender)))})", 400);

                var sizeCharts = await _db.SizeCharts
                    .Where(s => s.ProductType == productType && s.Gender == sizeGender)
                    .Select(s => new AISizeChartRowDto
                    {
                        Size = s.Size.ToString(),
                        // FIXED: SizeChart.MinHeight/MaxHeight/MinWeight/MaxWeight là decimal? (nullable)
                        // trong khi AISizeChartRowDto khai báo decimal (non-null) -> coalesce khi null.
                        // Min null = không giới hạn dưới -> 0; Max null = không giới hạn trên -> MaxValue
                        // (coalesce Max về 0 sẽ làm Python hiểu sai thành "tối đa = 0" và loại size đó ra).
                        MinHeight = s.MinHeight ?? 0,
                        MaxHeight = s.MaxHeight ?? decimal.MaxValue,
                        MinWeight = s.MinWeight ?? 0,
                        MaxWeight = s.MaxWeight ?? decimal.MaxValue
                    })
                    .ToListAsync();

                if (sizeCharts.Count == 0)
                    throw new AppException(
                        $"Chưa có bảng size cho loại sản phẩm '{request.ProductType}' - giới tính '{request.Gender}'", 404);

                var proxyRequest = new AISizeRecommendProxyRequest
                {
                    ProductType = request.ProductType,
                    Gender = request.Gender,
                    Height = request.Height,
                    Weight = request.Weight,
                    Chest = request.Chest,
                    Waist = request.Waist,
                    Hip = request.Hip,
                    SizeCharts = sizeCharts
                };

                result = await _aiClient.RecommendSizeAsync(proxyRequest);

                // Nếu khách đã đăng nhập, lưu lại số đo để lần sau khỏi nhập lại
                if (request.CustomerId.HasValue)
                {
                    var existing = await _db.CustomerMeasurements
                        .FirstOrDefaultAsync(m => m.CustomerId == request.CustomerId.Value);

                    if (existing == null)
                    {
                        _db.CustomerMeasurements.Add(new CustomerMeasurement
                        {
                            CustomerId = request.CustomerId.Value,
                            Height = request.Height,
                            Weight = request.Weight,
                            Chest = request.Chest ?? 0,
                            Waist = request.Waist ?? 0,
                            Hip = request.Hip ?? 0,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        existing.Height = request.Height;
                        existing.Weight = request.Weight;
                        if (request.Chest.HasValue) existing.Chest = request.Chest.Value;
                        if (request.Waist.HasValue) existing.Waist = request.Waist.Value;
                        if (request.Hip.HasValue) existing.Hip = request.Hip.Value;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }

                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                errorMessage = ex.Message;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                await SafeLogAsync(AIFeature.SizeRecommend, userId,
                    inputData: JsonSerializer.Serialize(request),
                    outputData: isSuccess ? JsonSerializer.Serialize(result) : null,
                    durationMs: (int)stopwatch.ElapsedMilliseconds,
                    isSuccess: isSuccess,
                    errorMessage: errorMessage);
            }

            return result;
        }

        // ===================== FORECAST =====================

        public async Task<InventoryForecastResponseDto> ForecastAsync(InventoryForecastRequestDto request, Guid? userId)
        {
            var stopwatch = Stopwatch.StartNew();
            InventoryForecastResponseDto result = null!; // FIXED: tránh CS0165
            bool isSuccess = true;
            string? errorMessage = null;

            try
            {
                var inventory = await _db.Inventories
                    .FirstOrDefaultAsync(i => i.VariantId == request.VariantId)
                    ?? throw new AppException("Không tìm thấy tồn kho cho variant này", 404);

                // Lấy lịch sử xuất kho (EXPORT) 90 ngày gần nhất, group theo ngày
                var fromDate = DateTime.UtcNow.AddDays(-90);
                var history = await _db.InventoryTransactions
                    .Where(t => t.VariantId == request.VariantId
                                && t.Type == InventoryTransactionType.EXPORT
                                && t.CreatedAt >= fromDate)
                    .GroupBy(t => t.CreatedAt.Date)
                    .Select(g => new AIForecastHistoryPointDto
                    {
                        Date = g.Key,
                        // Lưu ý: khi xuất kho, Quantity được lưu là số âm (xem OrderService.CreateAsync),
                        // nên cần đổi dấu để ra số lượng đã bán dương.
                        QuantitySold = -g.Sum(t => t.Quantity)
                    })
                    .OrderBy(p => p.Date)
                    .ToListAsync();

                if (history.Count < 30)
                {
                    result = new InventoryForecastResponseDto
                    {
                        VariantId = request.VariantId,
                        CurrentStock = inventory.Quantity,
                        WillRunOutInDays = null,
                        NeedReorder = inventory.Quantity <= inventory.MinStock,
                        Note = "Chưa đủ dữ liệu lịch sử bán hàng (cần tối thiểu 30 ngày có giao dịch xuất kho) để dự báo chính xác"
                    };
                    return result;
                }

                var proxyRequest = new AIForecastProxyRequest
                {
                    VariantId = request.VariantId,
                    HorizonDays = request.HorizonDays,
                    History = history
                };

                result = await _aiClient.ForecastAsync(proxyRequest);

                // Python chỉ dự báo NHU CẦU (demand), không biết tồn kho hiện tại
                // -> C# tự mô phỏng cạn kho dựa trên forecast curve + tồn kho thật
                var sortedForecast = result.Forecast.OrderBy(p => p.Date).ToList();
                double remainingStock = inventory.Quantity;
                int? willRunOutInDays = null;
                for (int i = 0; i < sortedForecast.Count; i++)
                {
                    remainingStock -= sortedForecast[i].PredictedQuantitySold;
                    if (remainingStock <= 0)
                    {
                        willRunOutInDays = i + 1;
                        break;
                    }
                }

                result.CurrentStock = inventory.Quantity;
                result.WillRunOutInDays = willRunOutInDays;
                // Cần nhập nếu: dự báo cạn trong vòng 14 ngày tới, HOẶC tồn kho đã chạm ngưỡng tối thiểu
                result.NeedReorder = (willRunOutInDays.HasValue && willRunOutInDays <= 14)
                                      || inventory.Quantity <= inventory.MinStock;
            }
            catch (Exception ex)
            {
                isSuccess = false;
                errorMessage = ex.Message;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                await SafeLogAsync(AIFeature.Forecast, userId,
                    inputData: JsonSerializer.Serialize(new { request.VariantId, request.HorizonDays }),
                    outputData: isSuccess ? JsonSerializer.Serialize(result) : null,
                    durationMs: (int)stopwatch.ElapsedMilliseconds,
                    isSuccess: isSuccess,
                    errorMessage: errorMessage);
            }

            return result;
        }

        // ===================== HELPER =====================

        /// <summary>
        /// Ghi log vào bảng AILogs. Lỗi khi ghi log KHÔNG được phép làm fail request chính,
        /// nên mọi exception ở đây bị nuốt (chỉ nên log ra console/file nếu cần debug).
        /// </summary>
        private async Task SafeLogAsync(
            AIFeature feature, Guid? userId,
            string? inputData, string? outputData,
            int durationMs, bool isSuccess, string? errorMessage)
        {
            try
            {
                _db.AILogs.Add(new AILog
                {
                    Feature = feature,
                    UserId = userId,
                    InputData = inputData,
                    OutputData = outputData,
                    Model = "gemini-2.0-flash", // TODO: đổi theo model thật bên Python service trả về nếu cần chính xác hơn
                    DurationMs = durationMs,
                    IsSuccess = isSuccess,
                    ErrorMessage = errorMessage != null && errorMessage.Length > 500 ? errorMessage[..500] : errorMessage
                });
                await _db.SaveChangesAsync();
            }
            catch
            {
                // Không throw lại - ghi log AI thất bại không nên ảnh hưởng tới response chính
            }




        }
        public async Task<TrendAnalysisResponseDto> GetTrendAnalysisAsync(
    TrendAnalysisRequestDto request, Guid userId)
        {
            // Lấy dữ liệu bán hàng trong khoảng thời gian
            var period = (int)(request.To - request.From).TotalDays;
            var prevFrom = request.From.AddDays(-period);

            var currentData = await GetSalesDataAsync(request.From, request.To, request.Category);
            var prevData = await GetSalesDataAsync(prevFrom, request.From, request.Category);

            var trends = currentData.Select(c =>
            {
                var prev = prevData.FirstOrDefault(p => p.Sku == c.Sku);
                var growthRate = prev == null || prev.TotalSold == 0
                    ? 100.0
                    : (c.TotalSold - prev.TotalSold) * 100.0 / prev.TotalSold;
                return new TrendAnalysisTrendItem(c.ProductName, c.Sku, c.TotalSold, c.Revenue, growthRate);
            }).ToList();

            var result = new TrendAnalysisResponseDto(
                TopTrends: trends.Where(t => t.GrowthRate >= 0).OrderByDescending(t => t.GrowthRate).Take(10).ToList(),
                DecliningItems: trends.Where(t => t.GrowthRate < 0).OrderBy(t => t.GrowthRate).Take(10).ToList(),
                Summary: $"Phân tích {currentData.Count} sản phẩm từ {request.From:dd/MM/yyyy} đến {request.To:dd/MM/yyyy}");

            await LogAIAsync("TrendAnalysis", userId, result);
            return result;
        }

        // Helper method (private)
        private async Task<List<(string ProductName, string Sku, int TotalSold, decimal Revenue)>>
            GetSalesDataAsync(DateTime from, DateTime to, string? category)
        {
            var query = _db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Variant).ThenInclude(v => v.Product).ThenInclude(p => p.Category)
                .Where(oi => oi.Order.Status == "Completed"
                          && oi.Order.CompletedAt >= from
                          && oi.Order.CompletedAt <= to);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(oi => oi.Variant.Product.Category.Name == category);

            return await query
                .GroupBy(oi => new { oi.ProductName, oi.Sku })
                .Select(g => ValueTuple.Create(g.Key.ProductName, g.Key.Sku ?? "", g.Sum(x => x.Quantity), g.Sum(x => x.LineTotal)))
                .ToListAsync();
        }

    }


}