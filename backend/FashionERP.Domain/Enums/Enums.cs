namespace FashionERP.Domain.Enums
{
    public enum UserRole
    {
        Admin,
        Manager,
        Sales,
        Warehouse,
        Accountant
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public enum ProductGender
    {
        Male,
        Female,
        Unisex,
        Kids
    }

    public enum EmployeeStatus
    {
        Active,
        Probation,
        Resigned
    }

    public enum AttendanceType
    {
        Normal,
        Late,
        EarlyLeave,
        Absent,
        Holiday
    }

    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum PayrollStatus
    {
        Draft,
        Confirmed,
        Paid
    }

    public enum ProductStatus
    {
        Active,
        Draft,
        Archived
    }

    public enum ProductSize
    {
        XS,
        S,
        M,
        L,
        XL,
        XXL,
        XXXL,
        FREE
    }

    public enum InventoryTransactionType
    {
        IMPORT,
        EXPORT,
        ADJUST,
        RETURN
    }

    public enum MemberLevel
    {
        Bronze,
        Silver,
        Gold,
        Platinum
    }

    public enum PromotionType
    {
        Percent,
        FixedAmount
    }

    public enum PaymentMethod
    {
        Cash,
        Transfer,
        Card
    }

    public enum OrderStatus
    {
        Pending,
        Completed,
        Cancelled,
        Returned,
        PartiallyReturned
    }

    public enum ReturnType
    {
        Refund,
        Exchange
    }

    public enum ReturnStatus
    {
        Pending,
        Completed
    }

    public enum SizeChartProductType
    {
        Shirt,
        Pants,
        Dress,
        Jacket,
        Skirt
    }

    public enum SizeChartGender
    {
        Male,
        Female,
        Unisex
    }

    public enum SizeChartSize
    {
        XS,
        S,
        M,
        L,
        XL,
        XXL
    }

    public enum AIFeature
    {
        Chatbot,
        SizeRecommend,
        Forecast,
        TrendAnalysis
    }
}

