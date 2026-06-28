using System;
using System.Threading.Tasks;
using FashionERP.Application.DTOs.AI;

namespace FashionERP.Application.Interfaces
{
    /// <summary>
    /// Tầng nghiệp vụ AI: build context từ DB (sản phẩm, khuyến mãi, SizeCharts, lịch sử kho),
    /// gọi sang Python AI Service qua IAIServiceClient, rồi ghi log vào bảng AILogs.
    /// Implementation: FashionERP.Infrastructure.Services.AIService
    /// </summary>
    public interface IAIService
    {
        Task<ChatbotResponseDto> ChatAsync(ChatbotRequestDto request, Guid? userId);

        Task<SizeRecommendResponseDto> RecommendSizeAsync(SizeRecommendRequestDto request, Guid? userId);

        Task<InventoryForecastResponseDto> ForecastAsync(InventoryForecastRequestDto request, Guid? userId);
        // Thêm vào interface IAIService
        Task<TrendAnalysisResponseDto> GetTrendAnalysisAsync(TrendAnalysisRequestDto request, Guid userId);
    }
}