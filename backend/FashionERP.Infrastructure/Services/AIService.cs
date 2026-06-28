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
using FashionERP.Infrastructure.Data;
using FashionERP.Domain.Enums;
using System.Collections.Generic;

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
            ChatbotResponseDto result = null!;
            bool isSuccess = true;
            string? errorMessage = null;

            try
            {
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

                var now = DateTime.UtcNow;
                var promotions = await _db.Promotions
                    .Where(pr => pr.IsActive && pr.StartDate <= now && pr.EndDate >= now)
                    .Select(pr => new AIPromotionContextDto
                    {
                        Code = pr.Code,
                        Name = pr.Name,
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
            SizeRecommendResponseDto result = null!;
            bool isSuccess = true;
            string? errorMessage = null;

            try
            {
                if (!Enum.TryParse<SizeChartProductType>(request.ProductType, true, out var productType))
                    throw new AppException(
                        $"Loại sản phẩm '{request.ProductType}' không hợp lệ", 400);

                if (!Enum.TryParse<SizeChartGender>(request.Gender, true, out var sizeGender))
                    throw new AppException(
                        $"Giới tính '{request.Gender}' không hợp lệ", 400);

                var sizeCharts = await _db.SizeCharts
                    .Where(s => s.ProductType == productType && s.Gender == sizeGender)
                    .Select(s => new AISizeChartRowDto
                    {
                        Size = s.Size.ToString(),
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
            InventoryForecastResponseDto result = null!;
            bool isSuccess = true;
            string? errorMessage = null;

            try
            {
                var inventory = await _db.Inventories
                    .FirstOrDefaultAsync(i => i.VariantId == request.VariantId)
                    ?? throw new AppException("Không tìm thấy tồn kho cho variant này", 404);

                var fromDate = DateTime.UtcNow.AddDays(-90);
                var history = await _db.InventoryTransactions
                    .Where(t => t.VariantId == request.VariantId
                                && t.Type == InventoryTransactionType.EXPORT
                                && t.CreatedAt >= fromDate)
                    .GroupBy(t => t.CreatedAt.Date)
                    .Select(g => new AIForecastHistoryPointDto
                    {
                        Date = g.Key,
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
                        Note = "Chưa đủ dữ liệu lịch sử bán hàng (cần tối thiểu 30 ngày) để dự báo chính xác"
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

        // ===================== TREND ANALYSIS =====================
        public async Task<TrendAnalysisResponseDto> GetTrendAnalysisAsync(
            TrendAnalysisRequestDto request, Guid userId)
        {
            var period = (int)(request.To - request.From).TotalDays;
            var prevFrom = request.From.AddDays(-period);

            var currentData = await GetSalesDataAsync(request.From, request.To, request.Category);
            var prevData = await GetSalesDataAsync(prevFrom, request.From, request.Category);

            var trends = currentData.Select(c =>
            {
                var prev = prevData.FirstOrDefault(p => p.Sku == c.Sku);

                // SỬA Ở ĐÂY: Dùng == default thay vì == null cho Tuple
                var growthRate = prev == default || prev.TotalSold == 0
                    ? 100.0
                    : (c.TotalSold - prev.TotalSold) * 100.0 / prev.TotalSold;

                return new TrendAnalysisTrendItem(c.ProductName, c.Sku, c.TotalSold, c.Revenue, growthRate);
            }).ToList();

            var result = new TrendAnalysisResponseDto(
                TopTrends: trends.Where(t => t.GrowthRate >= 0).OrderByDescending(t => t.GrowthRate).Take(10).ToList(),
                DecliningItems: trends.Where(t => t.GrowthRate < 0).OrderBy(t => t.GrowthRate).Take(10).ToList(),
                Summary: $"Phân tích {currentData.Count} sản phẩm từ {request.From:dd/MM/yyyy} đến {request.To:dd/MM/yyyy}");

            // SỬA Ở ĐÂY: Ẩn dòng gọi hàm Log chưa viết
            // await LogAIAsync("TrendAnalysis", userId, result); 

            return result;
        }

        // ===================== HELPER =====================
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
                    Model = "gemini-2.0-flash",
                    DurationMs = durationMs,
                    IsSuccess = isSuccess,
                    ErrorMessage = errorMessage != null && errorMessage.Length > 500 ? errorMessage[..500] : errorMessage
                });
                await _db.SaveChangesAsync();
            }
            catch
            {
            }
        }

        private async Task<List<(string ProductName, string Sku, int TotalSold, decimal Revenue)>>
            GetSalesDataAsync(DateTime from, DateTime to, string? category)
        {
            var query = _db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Variant).ThenInclude(v => v.Product).ThenInclude(p => p.Category)
                // SỬA Ở ĐÂY: Sửa thành oi.Order.Status == OrderStatus.Completed
                .Where(oi => oi.Order.Status == OrderStatus.Completed
                          && oi.Order.CompletedAt >= from
                          && oi.Order.CompletedAt <= to);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(oi => oi.Variant.Product.Category.Name == category);

            return await query
                // SỬA Ở ĐÂY: GroupBy Sku từ bảng Variant
                .GroupBy(oi => new { oi.ProductName, Sku = oi.Variant.Sku })
                .Select(g => ValueTuple.Create(g.Key.ProductName, g.Key.Sku ?? "", g.Sum(x => x.Quantity), g.Sum(x => x.LineTotal)))
                .ToListAsync();
        }
    }
}