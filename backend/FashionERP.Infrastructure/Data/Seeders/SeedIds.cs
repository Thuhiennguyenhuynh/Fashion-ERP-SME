using System;

namespace FashionERP.Infrastructure.Data.Seeders
{
    /// <summary>
    /// GUID cố định cho toàn bộ dữ liệu seed.
    /// Dùng fixed GUID thay vì NewGuid() để migration có thể chạy lại
    /// mà không tạo bản ghi trùng lặp.
    /// </summary>
    public static class SeedIds
    {
        // ── Departments ────────────────────────────────────────────────────
        public static readonly Guid Dept_BanHang = Guid.Parse("11111111-0001-0001-0001-000000000001");
        public static readonly Guid Dept_Kho = Guid.Parse("11111111-0001-0001-0001-000000000002");
        public static readonly Guid Dept_KeToan = Guid.Parse("11111111-0001-0001-0001-000000000003");
        public static readonly Guid Dept_Marketing = Guid.Parse("11111111-0001-0001-0001-000000000004");
        public static readonly Guid Dept_QuanLy = Guid.Parse("11111111-0001-0001-0001-000000000005");

        // ── Employees ─────────────────────────────────────────────────────
        public static readonly Guid Emp_Admin = Guid.Parse("22222222-0002-0002-0002-000000000001");
        public static readonly Guid Emp_Manager = Guid.Parse("22222222-0002-0002-0002-000000000002");
        public static readonly Guid Emp_Sales1 = Guid.Parse("22222222-0002-0002-0002-000000000003");
        public static readonly Guid Emp_Warehouse = Guid.Parse("22222222-0002-0002-0002-000000000004");
        public static readonly Guid Emp_Accountant = Guid.Parse("22222222-0002-0002-0002-000000000005");

        // ── Users ──────────────────────────────────────────────────────────
        public static readonly Guid User_Admin = Guid.Parse("33333333-0003-0003-0003-000000000001");
        public static readonly Guid User_Manager = Guid.Parse("33333333-0003-0003-0003-000000000002");
        public static readonly Guid User_Sales = Guid.Parse("33333333-0003-0003-0003-000000000003");
        public static readonly Guid User_Warehouse = Guid.Parse("33333333-0003-0003-0003-000000000004");
        public static readonly Guid User_Accountant = Guid.Parse("33333333-0003-0003-0003-000000000005");

        // ── Categories ────────────────────────────────────────────────────
        public static readonly Guid Cat_AoNam = Guid.Parse("44444444-0004-0004-0004-000000000001");
        public static readonly Guid Cat_QuanNam = Guid.Parse("44444444-0004-0004-0004-000000000002");
        public static readonly Guid Cat_AoNu = Guid.Parse("44444444-0004-0004-0004-000000000003");
        public static readonly Guid Cat_QuanNu = Guid.Parse("44444444-0004-0004-0004-000000000004");
        public static readonly Guid Cat_PhuKien = Guid.Parse("44444444-0004-0004-0004-000000000005");
        public static readonly Guid Cat_AoThun = Guid.Parse("44444444-0004-0004-0004-000000000006");
        public static readonly Guid Cat_AoSo = Guid.Parse("44444444-0004-0004-0004-000000000007");

        // ── Brands ────────────────────────────────────────────────────────
        public static readonly Guid Brand_Local = Guid.Parse("55555555-0005-0005-0005-000000000001");
        public static readonly Guid Brand_Nike = Guid.Parse("55555555-0005-0005-0005-000000000002");
        public static readonly Guid Brand_Adidas = Guid.Parse("55555555-0005-0005-0005-000000000003");

        // ── Products ──────────────────────────────────────────────────────
        public static readonly Guid Prod_AoThunTrang = Guid.Parse("66666666-0006-0006-0006-000000000001");
        public static readonly Guid Prod_AoThunDen = Guid.Parse("66666666-0006-0006-0006-000000000002");
        public static readonly Guid Prod_QuanJean = Guid.Parse("66666666-0006-0006-0006-000000000003");
        public static readonly Guid Prod_AoSoMi = Guid.Parse("66666666-0006-0006-0006-000000000004");
        public static readonly Guid Prod_AoDamNu = Guid.Parse("66666666-0006-0006-0006-000000000005");

        // ── ProductVariants ───────────────────────────────────────────────
        public static readonly Guid Var_AoThunTrang_M_White = Guid.Parse("77777777-0007-0007-0007-000000000001");
        public static readonly Guid Var_AoThunTrang_L_White = Guid.Parse("77777777-0007-0007-0007-000000000002");
        public static readonly Guid Var_AoThunDen_M_Black = Guid.Parse("77777777-0007-0007-0007-000000000003");
        public static readonly Guid Var_AoThunDen_L_Black = Guid.Parse("77777777-0007-0007-0007-000000000004");
        public static readonly Guid Var_QuanJean_30_Blue = Guid.Parse("77777777-0007-0007-0007-000000000005");
        public static readonly Guid Var_QuanJean_32_Blue = Guid.Parse("77777777-0007-0007-0007-000000000006");
        public static readonly Guid Var_AoSoMi_M_White = Guid.Parse("77777777-0007-0007-0007-000000000007");
        public static readonly Guid Var_AoDamNu_S_Red = Guid.Parse("77777777-0007-0007-0007-000000000008");

        // ── Customers ─────────────────────────────────────────────────────
        public static readonly Guid Cust_1 = Guid.Parse("88888888-0008-0008-0008-000000000001");
        public static readonly Guid Cust_2 = Guid.Parse("88888888-0008-0008-0008-000000000002");
        public static readonly Guid Cust_3 = Guid.Parse("88888888-0008-0008-0008-000000000003");

        // ── Promotions ────────────────────────────────────────────────────
        public static readonly Guid Promo_Welcome = Guid.Parse("99999999-0009-0009-0009-000000000001");
        public static readonly Guid Promo_Summer = Guid.Parse("99999999-0009-0009-0009-000000000002");
    }
}


