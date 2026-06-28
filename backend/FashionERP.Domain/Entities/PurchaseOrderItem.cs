using System;
using System.ComponentModel.DataAnnotations.Schema;
using FashionERP.Domain.Common;

namespace FashionERP.Domain.Entities
{
    public class PurchaseOrderItem : BaseEntity
    {
        public Guid PurchaseOrderId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; } = null!;

        public Guid VariantId { get; set; }
        public ProductVariant Variant { get; set; } = null!;

        public string ProductName { get; set; } = null!;
        public string Size { get; set; } = null!;
        public string Color { get; set; } = null!;

        public int OrderedQty { get; set; }
        public int ReceivedQty { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal LineTotal { get; set; }
    }
}