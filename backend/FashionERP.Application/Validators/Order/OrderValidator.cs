namespace FashionERP.Application.Validators.Order
{
    using FluentValidation;
    using FashionERP.Application.DTOs.Order;

    public class CreateOrderValidator : AbstractValidator<CreateOrderRequestDto>
    {
        private static readonly string[] ValidPayments = ["Cash", "Transfer", "Card"];

        public CreateOrderValidator()
        {
            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Phương thức thanh toán không được để trống")
                .Must(p => System.Array.Exists(ValidPayments, v => v == p))
                    .WithMessage("Phương thức thanh toán phải là Cash, Transfer hoặc Card");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Đơn hàng phải có ít nhất 1 sản phẩm");

            RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());

            RuleFor(x => x.Note)
                .MaximumLength(300).WithMessage("Ghi chú không được vượt quá 300 ký tự");
        }
    }

    public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemValidator()
        {
            RuleFor(x => x.VariantId)
                .NotEmpty().WithMessage("Biến thể sản phẩm không được để trống");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");
        }
    }

    public class CreateReturnValidator : AbstractValidator<CreateReturnRequestDto>
    {
        private static readonly string[] ValidReturnTypes = ["Refund", "Exchange"];

        public CreateReturnValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("Đơn hàng không được để trống");

            RuleFor(x => x.VariantId)
                .NotEmpty().WithMessage("Biến thể sản phẩm không được để trống");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Số lượng trả phải lớn hơn 0");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Lý do trả hàng không được để trống")
                .Length(3, 300).WithMessage("Lý do trả hàng phải có độ dài từ 3 đến 300 ký tự");

            RuleFor(x => x.ReturnType)
                .NotEmpty().WithMessage("Hình thức đổi trả không được để trống")
                .Must(r => System.Array.Exists(ValidReturnTypes, v => v == r))
                    .WithMessage("Hình thức đổi trả phải là Refund hoặc Exchange");

            RuleFor(x => x.RefundAmount)
                .GreaterThan(0).WithMessage("Số tiền hoàn phải lớn hơn 0")
                .When(x => x.ReturnType == "Refund");
        }
    }
}

