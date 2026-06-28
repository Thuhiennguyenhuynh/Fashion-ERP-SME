using System;
using System.Collections.Generic;
using FashionERP.Domain.Common;

namespace FashionERP.Domain.Entities
{
    public class Supplier : BaseEntity, ISoftDeletable
    {
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public decimal AccountsPayable { get; set; }

        public ICollection<PurchaseOrder> PurchaseOrders { get; set; }

        // 🔥 thiếu 2 field này nên trước đó bạn bị lỗi interface
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}