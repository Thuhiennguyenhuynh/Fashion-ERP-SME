namespace FashionERP.Application.Validators.HR
{
    using FluentValidation;
    using FashionERP.Application.DTOs.HR;

    public class CreateAttendanceManualValidator : AbstractValidator<CreateAttendanceManualDto>
    {
        private static readonly string[] ValidTypes =
            ["Normal", "Late", "EarlyLeave", "Absent", "Holiday"];

        public CreateAttendanceManualValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Nhân viên không được để trống");

            RuleFor(x => x.WorkDate)
                .NotEmpty().WithMessage("Ngày làm việc không được để trống")
                .LessThanOrEqualTo(System.DateTime.Today)
                    .WithMessage("Ngày làm việc không được ở tương lai");

            RuleFor(x => x.Type)
                .Must(t => System.Array.Exists(ValidTypes, v => v == t))
                    .WithMessage("Loại chấm công phải là Normal, Late, EarlyLeave, Absent hoặc Holiday");

            RuleFor(x => x.CheckOut)
                .GreaterThan(x => x.CheckIn)
                    .WithMessage("Giờ ra phải sau giờ vào")
                .When(x => x.CheckIn.HasValue && x.CheckOut.HasValue);

            RuleFor(x => x.Note)
                .MaximumLength(300).WithMessage("Ghi chú không được vượt quá 300 ký tự");
        }
    }

    public class CreateLeaveValidator : AbstractValidator<CreateLeaveRequestDto>
    {
        public CreateLeaveValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Nhân viên không được để trống");

            RuleFor(x => x.FromDate)
                .NotEmpty().WithMessage("Ngày bắt đầu nghỉ không được để trống");

            RuleFor(x => x.ToDate)
                .NotEmpty().WithMessage("Ngày kết thúc nghỉ không được để trống")
                .GreaterThanOrEqualTo(x => x.FromDate)
                    .WithMessage("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu nghỉ");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Lý do nghỉ không được để trống")
                .Length(3, 300).WithMessage("Lý do nghỉ phải có độ dài từ 3 đến 300 ký tự");
        }
    }

    public class ApproveLeaveValidator : AbstractValidator<ApproveLeaveRequestDto>
    {
        private static readonly string[] ValidStatuses = ["Approved", "Rejected"];

        public ApproveLeaveValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Trạng thái duyệt không được để trống")
                .Must(s => System.Array.Exists(ValidStatuses, v => v == s))
                    .WithMessage("Trạng thái phải là Approved hoặc Rejected");
        }
    }

    public class GeneratePayrollValidator : AbstractValidator<GeneratePayrollRequestDto>
    {
        public GeneratePayrollValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Nhân viên không được để trống");

            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12).WithMessage("Tháng phải từ 1 đến 12");

            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100).WithMessage("Năm phải từ 2000 đến 2100");

            RuleFor(x => x.Allowance)
                .GreaterThanOrEqualTo(0).WithMessage("Phụ cấp phải >= 0");

            RuleFor(x => x.Deduction)
                .GreaterThanOrEqualTo(0).WithMessage("Khấu trừ phải >= 0");
        }
    }
}

