using System;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Ghi log mỗi lần gọi AI (Chatbot, Size Recommend, Forecast, Trend Analysis)
    /// </summary>
    public class AILog : BaseEntity
    {
        [Required(ErrorMessage = "Tính năng AI không được để trống")]
        [EnumDataType(typeof(AIFeature),
            ErrorMessage = "Tính năng AI phải là Chatbot, SizeRecommend, Forecast hoặc TrendAnalysis")]
        public AIFeature Feature { get; set; }

        public Guid? UserId { get; set; }
        public virtual User? User { get; set; }

        /// <summary>JSON string - dữ liệu đầu vào</summary>
        public string? InputData { get; set; }

        /// <summary>JSON string - kết quả trả về</summary>
        public string? OutputData { get; set; }

        /// <summary>gemini-2.0-flash / sklearn-knn...</summary>
        [StringLength(50)]
        public string? Model { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số token sử dụng phải >= 0")]
        public int? TokensUsed { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thời gian xử lý phải >= 0")]
        public int? DurationMs { get; set; }

        public bool IsSuccess { get; set; } = true;

        [StringLength(500, ErrorMessage = "Thông báo lỗi không được vượt quá 500 ký tự")]
        public string? ErrorMessage { get; set; }
    }
}

    