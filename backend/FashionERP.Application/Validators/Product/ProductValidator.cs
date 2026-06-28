namespace FashionERP.Application.Validators.Product
{
    using FluentValidation;
    using FashionERP.Application.DTOs.Product;
    using FashionERP.Domain.Common;
 
    public class CreateProductValidator : AbstractValidator<CreateProductRequestDto>
    {
        private static readonly string[] ValidGenders = ["Male", "Female", "Unisex", "Kids"];
        private static readonly string[] ValidStatuses = ["Active", "Draft", "Archived"];
 
        public CreateProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên sản phẩm không được để trống")
                .Length(2, 200).WithMessage("Tên sản phẩm phải có độ dài từ 2 đến 200 ký tự");
 
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Danh mục không được để trống");
 
            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Đối tượng sử dụng không được để trống")
                .Must(g => System.Array.Exists(ValidGenders, v => v == g))
                    .WithMessage("Đối tượng phải là Male, Female, Unisex hoặc Kids");
 
            RuleFor(x => x.BasePrice)
                .GreaterThan(0).WithMessage("Giá bán cơ bản phải lớn hơn 0");
 
            RuleFor(x => x.Status)
                .Must(s => System.Array.Exists(ValidStatuses, v => v == s))
                    .WithMessage("Trạng thái phải là Active, Draft hoặc Archived")
                .When(x => !string.IsNullOrEmpty(x.Status));
        }
    }
 
    public class CreateVariantValidator : AbstractValidator<CreateVariantRequestDto>
    {
        private static readonly string[] ValidSizes =
            ["XS", "S", "M", "L", "XL", "XXL", "XXXL", "FREE"];
 
        public CreateVariantValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Sản phẩm không được để trống");
 
            RuleFor(x => x.Size)
                .NotEmpty().WithMessage("Kích cỡ không được để trống")
                .Must(s => System.Array.Exists(ValidSizes, v => v == s))
                    .WithMessage("Kích cỡ phải là XS, S, M, L, XL, XXL, XXXL hoặc FREE");
 
            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("Màu sắc không được để trống")
                .MaximumLength(50).WithMessage("Màu sắc không được vượt quá 50 ký tự");
 
            RuleFor(x => x.ColorHex)
                .Matches(ValidationConstants.ColorHexPattern)
                    .WithMessage("Mã màu HEX phải có dạng #RRGGBB, ví dụ: #FF0000")
                .When(x => !string.IsNullOrEmpty(x.ColorHex));
 
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Giá phải lớn hơn 0")
                .When(x => x.Price.HasValue);
 
            RuleFor(x => x.Barcode)
                .Matches(ValidationConstants.BarcodePattern)
                    .WithMessage("Barcode phải là mã EAN-13 gồm đúng 13 số")
                .When(x => !string.IsNullOrEmpty(x.Barcode));
        }
    }
}
 
 
