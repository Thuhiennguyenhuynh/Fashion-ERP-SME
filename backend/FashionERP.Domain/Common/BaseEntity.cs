using System;

namespace FashionERP.Domain.Common
{
    /// <summary>
    /// Lớp cơ sở cho tất cả entity: chứa Id, CreatedAt, UpdatedAt
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
 
}

