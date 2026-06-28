using System;
using System.Collections.Generic;

namespace FashionERP.Application.DTOs.AI
{
    // ===================== CHATBOT =====================

    /// <summary>Request gửi lên AIController cho tính năng chatbot tư vấn</summary>
    public class ChatbotRequestDto
    {
        public string Message { get; set; } = string.Empty;

        /// <summary>Lịch sử hội thoại gần nhất (tối đa ~10 lượt) để giữ context</summary>
        public List<ChatMessageDto>? History { get; set; }
    }

    public class ChatMessageDto
    {
        /// <summary>"user" hoặc "assistant"</summary>
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
    }

    public class ChatbotResponseDto
    {
        public string Reply { get; set; } = string.Empty;

        /// <summary>Danh sách sản phẩm gợi ý kèm theo (nếu AI có đề xuất)</summary>
        public List<SuggestedProductDto>? SuggestedProducts { get; set; }
    }

    public class SuggestedProductDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string? MainImageUrl { get; set; }
    }

    // ===================== SIZE RECOMMEND =====================

    public class SizeRecommendRequestDto
    {
        public string ProductType { get; set; } = string.Empty; // áo, quần, ...
        public string Gender { get; set; } = string.Empty;
        public decimal Height { get; set; }
        public decimal Weight { get; set; }
        public decimal? Chest { get; set; }
        public decimal? Waist { get; set; }
        public decimal? Hip { get; set; }

        /// <summary>Nếu khách đã đăng nhập, lưu lại số đo vào CustomerMeasurements</summary>
        public Guid? CustomerId { get; set; }
    }

    public class SizeRecommendResponseDto
    {
        public string RecommendedSize { get; set; } = string.Empty;
        public double Confidence { get; set; } // 0..1
        public string Explanation { get; set; } = string.Empty; // Gemini sinh giải thích tiếng Việt
        public List<SizeAlternativeDto>? Alternatives { get; set; }
    }

    public class SizeAlternativeDto
    {
        public string Size { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    // ===================== FORECAST =====================

    public class InventoryForecastRequestDto
    {
        public Guid VariantId { get; set; }

        /// <summary>Số ngày muốn dự báo tới, mặc định 30</summary>
        public int HorizonDays { get; set; } = 30;
    }

    public class InventoryForecastPointDto
    {
        public DateTime Date { get; set; }
        public double PredictedQuantitySold { get; set; }
    }

    public class InventoryForecastResponseDto
    {
        public Guid VariantId { get; set; }
        public int CurrentStock { get; set; }
        public List<InventoryForecastPointDto> Forecast { get; set; } = new();

        /// <summary>Số ngày ước tính tới khi hết hàng, null nếu không đủ dữ liệu dự báo</summary>
        public int? WillRunOutInDays { get; set; }

        public bool NeedReorder { get; set; }
        public string? Note { get; set; }
    }

    // ===================== Payload nội bộ gọi sang Python AI service =====================

    /// <summary>Payload thực tế gửi sang Python FastAPI (khác request từ FE vì có kèm context nội bộ)</summary>
    public class AIChatbotProxyRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatMessageDto>? History { get; set; }
        public List<AIProductContextDto> ProductContext { get; set; } = new();
        public List<AIPromotionContextDto> PromotionContext { get; set; } = new();
    }

    public class AIProductContextDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public decimal BasePrice { get; set; }
        public int TotalStock { get; set; }
    }

    public class AIPromotionContextDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
    }

    public class AISizeRecommendProxyRequest
    {
        public string ProductType { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public decimal Height { get; set; }
        public decimal Weight { get; set; }
        public decimal? Chest { get; set; }
        public decimal? Waist { get; set; }
        public decimal? Hip { get; set; }

        /// <summary>SizeCharts liên quan để Python không cần tự query DB</summary>
        public List<AISizeChartRowDto> SizeCharts { get; set; } = new();
    }

    public class AISizeChartRowDto
    {
        public string Size { get; set; } = string.Empty;
        public decimal MinHeight { get; set; }
        public decimal MaxHeight { get; set; }
        public decimal MinWeight { get; set; }
        public decimal MaxWeight { get; set; }
    }

    public class AIForecastProxyRequest
    {
        public Guid VariantId { get; set; }
        public int HorizonDays { get; set; } = 30;

        /// <summary>Lịch sử bán ra theo ngày (chỉ tính EXPORT), tối thiểu nên có ~30 điểm</summary>
        public List<AIForecastHistoryPointDto> History { get; set; } = new();
    }

    public class AIForecastHistoryPointDto
    {
        public DateTime Date { get; set; }
        public int QuantitySold { get; set; }
    }

    // Thêm vào file AIDTOs.cs
    public record TrendAnalysisRequestDto(
        DateTime From,
        DateTime To,
        string? Category = null);

    public record TrendAnalysisTrendItem(
        string ProductName,
        string Sku,
        int TotalSold,
        decimal Revenue,
        double GrowthRate);

    public record TrendAnalysisResponseDto(
        List<TrendAnalysisTrendItem> TopTrends,
        List<TrendAnalysisTrendItem> DecliningItems,
        string Summary);


    // ─── Trend Analysis Proxy (cho IAIServiceClient nếu cần forward Python) ──────
    public class AITrendAnalysisProxyRequest
    {
        /// <summary>Từ ngày (UTC)</summary>
        public DateTime From { get; set; }

        /// <summary>Đến ngày (UTC)</summary>
        public DateTime To { get; set; }

        /// <summary>Lọc theo danh mục sản phẩm (nullable)</summary>
        public string? Category { get; set; }

        /// <summary>Dữ liệu bán hàng kỳ hiện tại để Python phân tích</summary>
        public List<AITrendSalesPointDto> CurrentPeriodData { get; set; } = new();

        /// <summary>Dữ liệu bán hàng kỳ trước để Python so sánh trend</summary>
        public List<AITrendSalesPointDto> PreviousPeriodData { get; set; } = new();
    }

    public class AITrendSalesPointDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }
}