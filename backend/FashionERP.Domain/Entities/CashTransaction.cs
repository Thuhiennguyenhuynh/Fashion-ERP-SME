using System;
using System.ComponentModel.DataAnnotations.Schema;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Sổ quỹ thu/chi (Cash Management). Mọi dòng tiền thực tế ra/vào doanh nghiệp
    /// (bán hàng, thanh toán NCC, lương, chi phí mặt bằng, thu/chi thủ công...) đều
    /// phải đi qua bảng này để tính được số dư quỹ tại bất kỳ thời điểm (BalanceAfter).
    /// </summary>
    public class CashTransaction : BaseEntity, ISoftDeletable
    {
        public CashTransactionType Type { get; set; }

        /// <summary>Danh mục: "Bán hàng", "Mua NCC", "Lương", "Mặt bằng", "Khác"...</summary>
        public string Category { get; set; } = null!;

        [Column(TypeName = "decimal(15,2)")]
        public decimal Amount { get; set; }

        public string? Note { get; set; }

        /// <summary>Nguồn phát sinh: "Order" | "PurchaseOrder" | "Payroll" | "Manual"</summary>
        public string? RefType { get; set; }
        public Guid? RefId { get; set; }

        /// <summary>Số dư quỹ ngay sau giao dịch này (snapshot, phục vụ audit + tra cứu nhanh)</summary>
        [Column(TypeName = "decimal(15,2)")]
        public decimal BalanceAfter { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
