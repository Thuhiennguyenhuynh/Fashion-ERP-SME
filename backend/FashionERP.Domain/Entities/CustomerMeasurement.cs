using System;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Số đo cơ thể khách hàng - dùng cho AI gợi ý size (1 khách - 1 dòng, UNIQUE CustomerId)
    /// </summary>
    public class CustomerMeasurement : BaseEntity
    {
        [Required(ErrorMessage = "Khách hàng không được để trống")]
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        /// <summary>Chiều cao (cm)</summary>
        [Range(100, 250, ErrorMessage = "Chiều cao phải từ 100 đến 250 cm")]
        public decimal? Height { get; set; }

        /// <summary>Cân nặng (kg)</summary>
        [Range(30, 200, ErrorMessage = "Cân nặng phải từ 30 đến 200 kg")]
        public decimal? Weight { get; set; }

        /// <summary>Vòng ngực (cm)</summary>
        [Range(0, 300, ErrorMessage = "Vòng ngực phải từ 0 đến 300 cm")]
        public decimal? Chest { get; set; }

        /// <summary>Vòng eo (cm)</summary>
        [Range(0, 300, ErrorMessage = "Vòng eo phải từ 0 đến 300 cm")]
        public decimal? Waist { get; set; }

        /// <summary>Vòng hông (cm)</summary>
        [Range(0, 300, ErrorMessage = "Vòng hông phải từ 0 đến 300 cm")]
        public decimal? Hip { get; set; }

        [Required(ErrorMessage = "Thời gian cập nhật không được để trống")]
        public DateTime UpdatedAtMeasurement { get; set; } = DateTime.UtcNow;
    }
}

