using System;

namespace FashionERP.Domain.Common
{
    /// <summary>Entity nào cần soft-delete (Product, Customer, Employee, Promotion...) thì implement
    /// interface này. Không phá vỡ entity hiện tại — chỉ thêm 2 field.</summary>
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }

    /// <summary>Hàm dùng chung cho mọi Service, thay cho việc tự set IsDeleted/DeletedAt thủ công.</summary>
    public static class SoftDeleteExtensions
    {
        public static void MarkDeleted<T>(this T entity) where T : ISoftDeletable
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
        }

        public static void Restore<T>(this T entity) where T : ISoftDeletable
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
        }
    }
}