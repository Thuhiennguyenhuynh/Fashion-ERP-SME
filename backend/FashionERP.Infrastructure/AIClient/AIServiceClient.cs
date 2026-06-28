using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.AI;
using FashionERP.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FashionERP.Infrastructure.AIClient
{
    /// <summary>
    /// Client gọi HTTP sang Python AI Service (FastAPI), base URL lấy từ config "AiService:BaseUrl".
    /// Đăng ký DI: builder.Services.AddHttpClient("AiService", ...) trong Program.cs.
    /// </summary>
    public class AIServiceClient : IAIServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AIServiceClient> _logger;
        private const string ClientName = "AiService";

        public AIServiceClient(IHttpClientFactory httpClientFactory, ILogger<AIServiceClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<ChatbotResponseDto> ChatAsync(AIChatbotProxyRequest request, CancellationToken ct = default)
        {
            return await PostAsync<AIChatbotProxyRequest, ChatbotResponseDto>("/chatbot", request, ct);
        }

        public async Task<SizeRecommendResponseDto> RecommendSizeAsync(AISizeRecommendProxyRequest request, CancellationToken ct = default)
        {
            return await PostAsync<AISizeRecommendProxyRequest, SizeRecommendResponseDto>("/size-recommend", request, ct);
        }

        public async Task<InventoryForecastResponseDto> ForecastAsync(AIForecastProxyRequest request, CancellationToken ct = default)
        {
            return await PostAsync<AIForecastProxyRequest, InventoryForecastResponseDto>("/forecast", request, ct);
        }

        private async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest payload, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient(ClientName);
            try
            {
                var response = await client.PostAsJsonAsync(path, payload, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("AI service trả lỗi {StatusCode} tại {Path}: {Body}",
                        response.StatusCode, path, body);
                    throw new AppException(
                        "Dịch vụ AI hiện không phản hồi được, vui lòng thử lại sau", 503);
                }

                var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
                return result ?? throw new AppException("Dịch vụ AI trả về dữ liệu không hợp lệ", 502);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                // Timeout của HttpClient (không phải do người dùng hủy request)
                _logger.LogError(ex, "AI service timeout tại {Path}", path);
                throw new AppException("Dịch vụ AI phản hồi quá lâu, vui lòng thử lại", 504);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Không thể kết nối tới AI service tại {Path}", path);
                throw new AppException("Không thể kết nối tới dịch vụ AI", 503);
            }
        }
        // ─── Thêm method này vào class AIServiceClient ─────────────────────────────

        public async Task<TrendAnalysisResponseDto> GetTrendAnalysisAsync(
            AITrendAnalysisProxyRequest request,
            CancellationToken ct = default)
        {
            return await PostAsync<AITrendAnalysisProxyRequest, TrendAnalysisResponseDto>(
                "/trend-analysis", request, ct);
        }
    }
}