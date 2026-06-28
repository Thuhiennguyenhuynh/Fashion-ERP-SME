namespace FashionERP.Domain.Common
{
    /// <summary>
    /// Tập trung tất cả regex pattern + thông báo lỗi validate dùng chung
    /// cho Entity (Data Annotations) và DTO (FluentValidation)
    /// </summary>
    public static class ValidationConstants
    {
        // ===================== REGEX PATTERNS =====================

        /// <summary>Email chuẩn RFC 5322 (rút gọn, đủ dùng thực tế)</summary>
        public const string EmailPattern =
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        /// <summary>Số điện thoại Việt Nam: 10 số, bắt đầu bằng 0</summary>
        public const string PhonePattern = @"^0[0-9]{9}$";

        /// <summary>Mã màu HEX dạng #RRGGBB</summary>
        public const string ColorHexPattern = @"^#[0-9A-Fa-f]{6}$";

        /// <summary>Mã sản phẩm: PROD-YYYY-XXXX</summary>
        public const string ProductCodePattern = @"^PROD-\d{4}-\d{4,}$";

        /// <summary>SKU: chữ hoa/số, dấu gạch ngang, ví dụ PROD001-M-RED</summary>
        public const string SkuPattern = @"^[A-Z0-9]+-[A-Z0-9]+-[A-Z0-9]+$";

        /// <summary>Barcode EAN-13: đúng 13 số</summary>
        public const string BarcodePattern = @"^\d{13}$";

        /// <summary>Mã đơn hàng: ORD-YYYYMMDD-XXX</summary>
        public const string OrderCodePattern = @"^ORD-\d{8}-\d{3,}$";

        /// <summary>Mã khuyến mãi: chữ hoa và số, 3-50 ký tự, không khoảng trắng</summary>
        public const string PromotionCodePattern = @"^[A-Z0-9]{3,50}$";

        /// <summary>Slug URL-friendly: chữ thường, số, gạch ngang</summary>
        public const string SlugPattern = @"^[a-z0-9]+(?:-[a-z0-9]+)*$";

        /// <summary>Mật khẩu: tối thiểu 8 ký tự, có hoa, thường, số</summary>
        public const string PasswordPattern =
            @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$";

        /// <summary>Mã nhân viên (nếu cần) EMP-XXXX</summary>
        public const string EmployeeCodePattern = @"^EMP-\d{4,}$";

        // ===================== MESSAGES =====================

        public const string MsgRequired = "{0} không được để trống";
        public const string MsgEmailInvalid = "{0} không đúng định dạng email (ví dụ: ten@example.com)";
        public const string MsgPhoneInvalid = "{0} phải là số điện thoại Việt Nam hợp lệ gồm 10 số, bắt đầu bằng số 0";
        public const string MsgColorHexInvalid = "{0} phải có dạng mã màu HEX, ví dụ: #FF0000";
        public const string MsgProductCodeInvalid = "{0} phải có định dạng PROD-YYYY-XXXX, ví dụ: PROD-2025-0001";
        public const string MsgSkuInvalid = "{0} phải có định dạng SKU-SIZE-COLOR, ví dụ: PROD001-M-RED";
        public const string MsgBarcodeInvalid = "{0} phải là mã barcode EAN-13 gồm đúng 13 số";
        public const string MsgOrderCodeInvalid = "{0} phải có định dạng ORD-YYYYMMDD-XXX, ví dụ: ORD-20250601-001";
        public const string MsgPromotionCodeInvalid = "{0} chỉ được chứa chữ hoa và số, từ 3 đến 50 ký tự, không khoảng trắng";
        public const string MsgSlugInvalid = "{0} chỉ được chứa chữ thường, số và dấu gạch ngang, ví dụ: ao-thun-nam";
        public const string MsgPasswordInvalid = "{0} phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường và số";
        public const string MsgStringLength = "{0} phải có độ dài từ {2} đến {1} ký tự";
        public const string MsgRange = "{0} phải nằm trong khoảng từ {1} đến {2}";
        public const string MsgMinValue = "{0} phải lớn hơn hoặc bằng {1}";
        public const string MsgGreaterThanZero = "{0} phải lớn hơn 0";
    }
}

