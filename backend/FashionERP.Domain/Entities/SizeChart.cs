using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Bảng size chuẩn - dùng cho AI gợi ý size theo số đo khách hàng.
    /// UNIQUE (ProductType, Gender, Size) cấu hình tại DbContext.
    /// </summary>
    public class SizeChart : BaseEntity
    {
        [Required(ErrorMessage = "Loại sản phẩm không được để trống")]
        [EnumDataType(typeof(SizeChartProductType),
            ErrorMessage = "Loại sản phẩm phải là Shirt, Pants, Dress, Jacket hoặc Skirt")]
        public SizeChartProductType ProductType { get; set; }

        [Required(ErrorMessage = "Đối tượng không được để trống")]
        [EnumDataType(typeof(SizeChartGender), ErrorMessage = "Đối tượng phải là Male, Female hoặc Unisex")]
        public SizeChartGender Gender { get; set; }

        [Required(ErrorMessage = "Kích cỡ không được để trống")]
        [EnumDataType(typeof(SizeChartSize), ErrorMessage = "Kích cỡ phải là XS, S, M, L, XL hoặc XXL")]
        public SizeChartSize Size { get; set; }

        /// <summary>Chiều cao (cm)</summary>
        [Range(0, 300, ErrorMessage = "Chiều cao tối thiểu phải từ 0 đến 300 cm")]
        public decimal? MinHeight { get; set; }

        [Range(0, 300, ErrorMessage = "Chiều cao tối đa phải từ 0 đến 300 cm")]
        public decimal? MaxHeight { get; set; }

        /// <summary>Cân nặng (kg)</summary>
        [Range(0, 300, ErrorMessage = "Cân nặng tối thiểu phải từ 0 đến 300 kg")]
        public decimal? MinWeight { get; set; }

        [Range(0, 300, ErrorMessage = "Cân nặng tối đa phải từ 0 đến 300 kg")]
        public decimal? MaxWeight { get; set; }

        /// <summary>Vòng ngực (cm)</summary>
        [Range(0, 300, ErrorMessage = "Vòng ngực tối thiểu phải từ 0 đến 300 cm")]
        public decimal? MinChest { get; set; }

        [Range(0, 300, ErrorMessage = "Vòng ngực tối đa phải từ 0 đến 300 cm")]
        public decimal? MaxChest { get; set; }

        /// <summary>Vòng eo (cm)</summary>
        [Range(0, 300, ErrorMessage = "Vòng eo tối thiểu phải từ 0 đến 300 cm")]
        public decimal? MinWaist { get; set; }

        [Range(0, 300, ErrorMessage = "Vòng eo tối đa phải từ 0 đến 300 cm")]
        public decimal? MaxWaist { get; set; }

        /// <summary>Vòng hông (cm)</summary>
        [Range(0, 300, ErrorMessage = "Vòng hông tối thiểu phải từ 0 đến 300 cm")]
        public decimal? MinHip { get; set; }

        [Range(0, 300, ErrorMessage = "Vòng hông tối đa phải từ 0 đến 300 cm")]
        public decimal? MaxHip { get; set; }
    }
}

