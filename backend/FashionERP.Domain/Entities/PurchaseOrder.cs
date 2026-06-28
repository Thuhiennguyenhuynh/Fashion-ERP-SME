using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using FashionERP.Domain.Enums;
using FashionERP.Domain.Common; // Đảm bảo gọi đến thư mục chứa ISoftDeletable

namespace FashionERP.Domain.Entities
{
    public class PurchaseOrder : BaseEntity, ISoftDeletable
    {
        public string PoCode { get; set; } = null!;
        public Guid SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

        [Column(TypeName = "decimal(15,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal PaidAmount { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal DebtAmount { get; set; } = 0;

        public DateTime? ExpectedDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string? Note { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}