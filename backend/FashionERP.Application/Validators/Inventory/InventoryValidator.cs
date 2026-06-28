namespace FashionERP.Application.Validators.Inventory
{
    using FluentValidation;
    using FashionERP.Application.DTOs.Inventory;

    public class ImportStockValidator : AbstractValidator<ImportStockRequestDto>
    {
        public ImportStockValidator()
        {
            RuleFor(x => x.VariantId)
                .NotEmpty().WithMessage("Biến thể sản phẩm không được để trống");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Số lượng nhập phải lớn hơn 0");

            RuleFor(x => x.UnitCost)
                .GreaterThanOrEqualTo(0).WithMessage("Giá nhập phải >= 0");

            RuleFor(x => x.Note)
                .MaximumLength(300).WithMessage("Ghi chú không được vượt quá 300 ký tự");
        }
    }

    public class AdjustStockValidator : AbstractValidator<AdjustStockRequestDto>
    {
        public AdjustStockValidator()
        {
            RuleFor(x => x.VariantId)
                .NotEmpty().WithMessage("Biến thể sản phẩm không được để trống");

            RuleFor(x => x.NewQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Số lượng điều chỉnh phải >= 0");

            RuleFor(x => x.Note)
                .MaximumLength(300).WithMessage("Ghi chú không được vượt quá 300 ký tự");
        }
    }
}


