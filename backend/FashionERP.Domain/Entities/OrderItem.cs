using System;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Chi tiết đơn hàng. Các trường ProductName/Size/Color/UnitPrice là SNAPSHOT
    /// tại thời điểm bán, không đổi theo dữ liệu sản phẩm về sau.
    /// CASCADE DELETE theo Order.
    /// </summary>
    public class OrderItem : BaseEntity
    {
        [Required(ErrorMessage = "Đơn hàng không được để trống")]
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        [Required(ErrorMessage = "Biến thể sản phẩm không được để trống")]
        public Guid VariantId { get; set; }
        public virtual ProductVariant Variant { get; set; } = null!;

        /// <summary>SNAPSHOT - tên sản phẩm lúc bán</summary>
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>SNAPSHOT</summary>
        [Required(ErrorMessage = "Kích cỡ không được để trống")]
        [StringLength(10)]
        public string Size { get; set; } = string.Empty;

        /// <summary>SNAPSHOT</summary>
        [Required(ErrorMessage = "Màu sắc không được để trống")]
        [StringLength(50)]
        public string Color { get; set; } = string.Empty;

        /// <summary>SNAPSHOT - giá bán tại thời điểm đó</summary>
        [Required(ErrorMessage = "Đơn giá không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        /// <summary>= UnitPrice * Quantity</summary>
        [Required(ErrorMessage = "Tổng tiền dòng không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tổng tiền dòng phải lớn hơn 0")]
        public decimal LineTotal { get; set; }
    }
}

