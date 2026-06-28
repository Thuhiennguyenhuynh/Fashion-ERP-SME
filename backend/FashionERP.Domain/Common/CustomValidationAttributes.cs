using System;
using System.ComponentModel.DataAnnotations;

namespace FashionERP.Domain.Common
{
    /// <summary>
    /// Validate giá trị int khác 0 (dùng cho InventoryTransaction.Quantity: + nhập / - xuất)
    /// </summary>
    public class NotZeroAttribute : ValidationAttribute
    {
        public NotZeroAttribute()
        {
            ErrorMessage = "{0} không được bằng 0";
        }

        public override bool IsValid(object? value)
        {
            if (value is int i) return i != 0;
            if (value is decimal d) return d != 0;
            if (value is double db) return db != 0;
            return true;
        }
    }

    /// <summary>
    /// Validate ngày kết thúc phải >= ngày bắt đầu (dùng cho Leave, Promotion)
    /// So sánh động bằng tên property thông qua reflection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
            ErrorMessage = "{0} phải lớn hơn hoặc bằng giá trị so sánh";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null)
                throw new ArgumentException($"Không tìm thấy thuộc tính {_comparisonProperty}");

            var comparisonValue = property.GetValue(validationContext.ObjectInstance);

            if (value is IComparable currentValue && comparisonValue is IComparable)
            {
                if (currentValue.CompareTo(comparisonValue) < 0)
                {
                    return new ValidationResult(
                        ErrorMessage ?? $"{validationContext.DisplayName} phải lớn hơn hoặc bằng {_comparisonProperty}",
                        new[] { validationContext.MemberName ?? string.Empty });
                }
            }

            return ValidationResult.Success;
        }
    }
}

