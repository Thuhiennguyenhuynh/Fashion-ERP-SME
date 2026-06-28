using System;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Chi phí vận hành (Nhập hàng / Lương / Mặt bằng / Điện nước / Marketing...)
    /// </summary>
    public class Expense : BaseEntity
    {
        [Required(ErrorMessage = "Loại chi phí không được để trống")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Loại chi phí phải có độ dài từ 2 đến 100 ký tự")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số tiền không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
        public decimal Amount { get; set; }

        [StringLength(300, ErrorMessage = "Mô tả không được vượt quá 300 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Ngày chi không được để trống")]
        public DateTime ExpenseDate { get; set; }

        [Required(ErrorMessage = "Người tạo chi phí không được để trống")]
      
        public virtual User Creator { get; set; } = null!;
    }
}

