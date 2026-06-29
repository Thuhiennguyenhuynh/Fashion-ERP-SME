namespace FashionERP.Application.Validators.Procurement
{
    using System;
    using FluentValidation;
    using FashionERP.Application.DTOs.Procurement;

    public class CreateSupplierValidator : AbstractValidator<CreateSupplierRequestDto>
    {
        public CreateSupplierValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên nhà cung cấp không được để trống")
                .Length(2, 200).WithMessage("Tên nhà cung cấp phải có độ dài từ 2 đến 200 ký tự");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Số điện thoại không được để trống")
                .Matches(@"^[0-9+\s-]{8,15}$").WithMessage("Số điện thoại không hợp lệ");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email không hợp lệ")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.TaxCode)
                .MaximumLength(20).WithMessage("Mã số thuế tối đa 20 ký tự");
        }
    }

    public class CreatePurchaseOrderValidator : AbstractValidator<CreatePurchaseOrderRequestDto>
    {
        public CreatePurchaseOrderValidator()
        {
            RuleFor(x => x.SupplierId)
                .NotEqual(Guid.Empty).WithMessage("Phải chọn nhà cung cấp");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Phiếu đặt hàng phải có ít nhất 1 sản phẩm");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.VariantId)
                    .NotEqual(Guid.Empty).WithMessage("Phải chọn biến thể sản phẩm");

                item.RuleFor(i => i.OrderedQty)
                    .GreaterThan(0).WithMessage("Số lượng đặt phải lớn hơn 0");

                item.RuleFor(i => i.UnitCost)
                    .GreaterThanOrEqualTo(0).WithMessage("Đơn giá nhập phải >= 0");
            });
        }
    }

    public class ReceivePurchaseOrderValidator : AbstractValidator<ReceivePurchaseOrderRequestDto>
    {
        public ReceivePurchaseOrderValidator()
        {
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Phải có ít nhất 1 dòng sản phẩm để nhận hàng");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.PurchaseOrderItemId)
                    .NotEqual(Guid.Empty).WithMessage("Dòng sản phẩm không hợp lệ");

                item.RuleFor(i => i.ReceivedQtyThisTime)
                    .GreaterThan(0).WithMessage("Số lượng nhận lần này phải lớn hơn 0");
            });
        }
    }

    public class PayPurchaseOrderValidator : AbstractValidator<PayPurchaseOrderRequestDto>
    {
        public PayPurchaseOrderValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Số tiền thanh toán phải lớn hơn 0");
        }
    }
}
