using System.Reflection;
using Microsoft.EntityFrameworkCore;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Domain.Common;

using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FashionERP.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IHttpContextAccessor httpContextAccessor
        ) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        // ===== Auth & HR =====
        public DbSet<User> Users => Set<User>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<Leave> Leaves => Set<Leave>();
        public DbSet<Payroll> Payrolls => Set<Payroll>();

        // ===== Products & Inventory =====
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

        // ===== Customers & Sales =====
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<CustomerMeasurement> CustomerMeasurements => Set<CustomerMeasurement>();
        public DbSet<Promotion> Promotions => Set<Promotion>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Return> Returns => Set<Return>();

        // ===== Accounting & AI =====
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<SizeChart> SizeCharts => Set<SizeChart>();
        public DbSet<AILog> AILogs => Set<AILog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUsers(modelBuilder);
            ConfigureDepartments(modelBuilder);
            ConfigureEmployees(modelBuilder);
            ConfigureAttendances(modelBuilder);
            ConfigureLeaves(modelBuilder);
            ConfigurePayrolls(modelBuilder);

            ConfigureCategories(modelBuilder);
            ConfigureBrands(modelBuilder);
            ConfigureProducts(modelBuilder);
            ConfigureProductImages(modelBuilder);
            ConfigureProductVariants(modelBuilder);
            ConfigureInventory(modelBuilder);
            ConfigureInventoryTransactions(modelBuilder);

            ConfigureCustomers(modelBuilder);
            ConfigureCustomerMeasurements(modelBuilder);
            ConfigurePromotions(modelBuilder);
            ConfigureOrders(modelBuilder);
            ConfigureOrderItems(modelBuilder);
            ConfigureReturns(modelBuilder);

            ConfigureExpenses(modelBuilder);
            ConfigureSizeCharts(modelBuilder);
            ConfigureAILogs(modelBuilder);

            // ===== SOFT DELETE: tự áp Global Query Filter cho MỌI entity =====
            // implement ISoftDeletable, không cần khai .HasQueryFilter() riêng từng entity.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(AppDbContext)
                        .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);
                    method.Invoke(null, new object[] { modelBuilder });
                }
            }
        }

        private static void SetSoftDeleteFilter<T>(ModelBuilder builder) where T : class, ISoftDeletable
            => builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);

        // ============================================================
        // 1. AUTH & HR
        // ============================================================

        private static void ConfigureUsers(ModelBuilder b)
        {
            b.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(x => x.Id);
                e.Property(x => x.Email).IsRequired().HasMaxLength(255);
                e.HasIndex(x => x.Email).IsUnique();
                e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
                e.Property(x => x.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();
                e.Property(x => x.RefreshToken).HasMaxLength(512);

                // 1-1 với Employee (nullable)
                e.HasOne(x => x.Employee)
                    .WithOne(x => x.User)
                    .HasForeignKey<User>(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private static void ConfigureDepartments(ModelBuilder b)
        {
            b.Entity<Department>(e =>
            {
                e.ToTable("Departments");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Name).IsUnique();
                e.Property(x => x.Description).HasMaxLength(500);

                // Manager (Employee) - không cascade để tránh multiple cascade path
                e.HasOne(x => x.Manager)
                    .WithMany()
                    .HasForeignKey(x => x.ManagerId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }

        private static void ConfigureEmployees(ModelBuilder b)
        {
            b.Entity<Employee>(e =>
            {
                e.ToTable("Employees");
                e.HasKey(x => x.Id);
                e.Property(x => x.FullName).IsRequired().HasMaxLength(150);
                e.Property(x => x.Phone).IsRequired().HasMaxLength(15);
                e.HasIndex(x => x.Phone).IsUnique();
                e.Property(x => x.Email).HasMaxLength(255);
                e.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");

                e.Property(x => x.Gender).HasConversion<string>().HasMaxLength(10);
                e.Property(x => x.Address).HasMaxLength(300);
                e.Property(x => x.Position).IsRequired().HasMaxLength(100);
                e.Property(x => x.BaseSalary).HasColumnType("decimal(15,2)");
                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                e.Property(x => x.AvatarUrl).HasMaxLength(500);
                e.Property(x => x.AvatarPublicId).HasMaxLength(200);

                e.HasOne(x => x.Department)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureAttendances(ModelBuilder b)
        {
            b.Entity<Attendance>(e =>
            {
                e.ToTable("Attendances");
                e.HasKey(x => x.Id);
                e.Property(x => x.TotalHours).HasColumnType("decimal(5,2)");
                e.Property(x => x.OvertimeHours).HasColumnType("decimal(5,2)");
                e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
                e.Property(x => x.Note).HasMaxLength(300);

                // INDEX: (employeeId, workDate) UNIQUE - 1 nhân viên 1 bản ghi/ngày
                e.HasIndex(x => new { x.EmployeeId, x.WorkDate }).IsUnique();

                e.HasOne(x => x.Employee)
                    .WithMany(emp => emp.Attendances)
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureLeaves(ModelBuilder b)
        {
            b.Entity<Leave>(e =>
            {
                e.ToTable("Leaves");
                e.HasKey(x => x.Id);
                e.Property(x => x.Reason).IsRequired().HasMaxLength(300);
                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

                e.HasOne(x => x.Employee)
                    .WithMany(emp => emp.Leaves)
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Approver)
                    .WithMany()
                    .HasForeignKey(x => x.ApprovedBy)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }

        private static void ConfigurePayrolls(ModelBuilder b)
        {
            b.Entity<Payroll>(e =>
            {
                e.ToTable("Payrolls");
                e.HasKey(x => x.Id);
                e.Property(x => x.WorkingDaysActual).HasColumnType("decimal(5,1)");
                e.Property(x => x.BaseSalary).HasColumnType("decimal(15,2)");
                e.Property(x => x.Allowance).HasColumnType("decimal(15,2)");
                e.Property(x => x.OvertimePay).HasColumnType("decimal(15,2)");
                e.Property(x => x.Deduction).HasColumnType("decimal(15,2)");
                e.Property(x => x.NetSalary).HasColumnType("decimal(15,2)");
                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

                // INDEX: (employeeId, month, year) UNIQUE
                e.HasIndex(x => new { x.EmployeeId, x.Month, x.Year }).IsUnique();

                e.HasOne(x => x.Employee)
                    .WithMany(emp => emp.Payrolls)
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // ============================================================
        // 2. PRODUCTS & INVENTORY
        // ============================================================

        private static void ConfigureCategories(ModelBuilder b)
        {
            b.Entity<Category>(e =>
            {
                e.ToTable("Categories");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Name).IsUnique();
                e.Property(x => x.Slug).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Slug).IsUnique();
                e.Property(x => x.Description).HasMaxLength(300);

                // Self-referencing - nested categories
                e.HasOne(x => x.Parent)
                    .WithMany(x => x.Children)
                    .HasForeignKey(x => x.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureBrands(ModelBuilder b)
        {
            b.Entity<Brand>(e =>
            {
                e.ToTable("Brands");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Name).IsUnique();
                e.Property(x => x.LogoUrl).HasMaxLength(500);
                e.Property(x => x.LogoPublicId).HasMaxLength(200);
                e.Property(x => x.Country).HasMaxLength(100);
            });
        }

        private static void ConfigureProducts(ModelBuilder b)
        {
            b.Entity<Product>(e =>
            {
                e.ToTable("Products");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Description).HasColumnType("nvarchar(max)");
                e.Property(x => x.Gender).HasConversion<string>().HasMaxLength(10);
                e.Property(x => x.BasePrice).HasColumnType("decimal(15,2)");
                e.Property(x => x.MainImageUrl).HasMaxLength(500);
                e.Property(x => x.MainImagePublicId).HasMaxLength(200);
                e.Property(x => x.Tags).HasMaxLength(500);
                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                e.Property(x => x.ProductCode).IsRequired().HasMaxLength(50);
                e.HasIndex(x => x.ProductCode).IsUnique();

                e.HasOne(x => x.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Brand)
                    .WithMany(br => br.Products)
                    .HasForeignKey(x => x.BrandId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private static void ConfigureProductImages(ModelBuilder b)
        {
            b.Entity<ProductImage>(e =>
            {
                e.ToTable("ProductImages");
                e.HasKey(x => x.Id);
                e.Property(x => x.ImageUrl).IsRequired().HasMaxLength(500);
                e.Property(x => x.PublicId).IsRequired().HasMaxLength(200);
                e.Property(x => x.AltText).HasMaxLength(200);

                // CASCADE DELETE theo Product
                e.HasOne(x => x.Product)
                    .WithMany(p => p.Images)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureProductVariants(ModelBuilder b)
        {
            b.Entity<ProductVariant>(e =>
            {
                e.ToTable("ProductVariants");
                e.HasKey(x => x.Id);
                e.Property(x => x.Size).HasConversion<string>().HasMaxLength(10).IsRequired();
                e.Property(x => x.Color).IsRequired().HasMaxLength(50);
                e.Property(x => x.ColorHex).HasMaxLength(7);
                e.Property(x => x.Sku).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Sku).IsUnique();
                e.Property(x => x.Barcode).HasMaxLength(20);
                e.HasIndex(x => x.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");
                e.Property(x => x.Price).HasColumnType("decimal(15,2)");
                e.Property(x => x.ImageUrl).HasMaxLength(500);
                e.Property(x => x.ImagePublicId).HasMaxLength(200);

                // INDEX: (productId, size, color) UNIQUE
                e.HasIndex(x => new { x.ProductId, x.Size, x.Color }).IsUnique();

                // CASCADE DELETE theo Product
                e.HasOne(x => x.Product)
                    .WithMany(p => p.Variants)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureInventory(ModelBuilder b)
        {
            b.Entity<Inventory>(e =>
            {
                e.ToTable("Inventory");
                e.HasKey(x => x.Id);
                e.Property(x => x.Location).HasMaxLength(100);
                e.Property(x => x.AvgCost).HasColumnType("decimal(15,2)");

                // 1 variant - 1 dòng tồn kho (UNIQUE)
                e.HasIndex(x => x.VariantId).IsUnique();

                e.HasOne(x => x.Variant)
                    .WithOne(v => v.Inventory)
                    .HasForeignKey<Inventory>(x => x.VariantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureInventoryTransactions(ModelBuilder b)
        {
            b.Entity<InventoryTransaction>(e =>
            {
                e.ToTable("InventoryTransactions");
                e.HasKey(x => x.Id);
                e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
                e.Property(x => x.UnitCost).HasColumnType("decimal(15,2)");
                e.Property(x => x.RefType).HasMaxLength(30);
                e.Property(x => x.Note).HasMaxLength(300);

                e.HasOne(x => x.Variant)
                    .WithMany(v => v.InventoryTransactions)
                    .HasForeignKey(x => x.VariantId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Creator)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // ============================================================
        // 3. CUSTOMERS & SALES
        // ============================================================

        private static void ConfigureCustomers(ModelBuilder b)
        {
            b.Entity<Customer>(e =>
            {
                e.ToTable("Customers");
                e.HasKey(x => x.Id);
                e.Property(x => x.FullName).IsRequired().HasMaxLength(150);
                e.Property(x => x.Phone).IsRequired().HasMaxLength(15);
                e.HasIndex(x => x.Phone).IsUnique();
                e.Property(x => x.Email).HasMaxLength(255);
                e.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
                e.Property(x => x.Gender).HasConversion<string>().HasMaxLength(10);
                e.Property(x => x.Address).HasMaxLength(300);
                e.Property(x => x.AvatarUrl).HasMaxLength(500);
                e.Property(x => x.AvatarPublicId).HasMaxLength(200);
                e.Property(x => x.MemberLevel).HasConversion<string>().HasMaxLength(20);
                e.Property(x => x.TotalSpent).HasColumnType("decimal(15,2)");
                e.Property(x => x.Note).HasMaxLength(300);
            });
        }

        private static void ConfigureCustomerMeasurements(ModelBuilder b)
        {
            b.Entity<CustomerMeasurement>(e =>
            {
                e.ToTable("CustomerMeasurements");
                e.HasKey(x => x.Id);
                e.Property(x => x.Height).HasColumnType("decimal(5,1)");
                e.Property(x => x.Weight).HasColumnType("decimal(5,1)");
                e.Property(x => x.Chest).HasColumnType("decimal(5,1)");
                e.Property(x => x.Waist).HasColumnType("decimal(5,1)");
                e.Property(x => x.Hip).HasColumnType("decimal(5,1)");

                // 1 khách - 1 dòng (UNIQUE)
                e.HasIndex(x => x.CustomerId).IsUnique();

                e.HasOne(x => x.Customer)
                    .WithOne(c => c.Measurement)
                    .HasForeignKey<CustomerMeasurement>(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigurePromotions(ModelBuilder b)
        {
            b.Entity<Promotion>(e =>
            {
                e.ToTable("Promotions");
                e.HasKey(x => x.Id);
                e.Property(x => x.Code).IsRequired().HasMaxLength(50);
                e.HasIndex(x => x.Code).IsUnique();
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
                e.Property(x => x.DiscountValue).HasColumnType("decimal(10,2)");
                e.Property(x => x.MaxDiscount).HasColumnType("decimal(15,2)");
                e.Property(x => x.MinOrderValue).HasColumnType("decimal(15,2)");
            });
        }

        private static void ConfigureOrders(ModelBuilder b)
        {
            b.Entity<Order>(e =>
            {
                e.ToTable("Orders");
                e.HasKey(x => x.Id);
                e.Property(x => x.OrderCode).IsRequired().HasMaxLength(30);
                e.HasIndex(x => x.OrderCode).IsUnique();
                e.Property(x => x.Subtotal).HasColumnType("decimal(15,2)");
                e.Property(x => x.DiscountAmount).HasColumnType("decimal(15,2)");
                e.Property(x => x.TaxAmount).HasColumnType("decimal(15,2)");
                e.Property(x => x.FinalAmount).HasColumnType("decimal(15,2)");
                e.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(20).IsRequired();
                e.Property(x => x.PromotionCode).HasMaxLength(50);
                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                e.Property(x => x.Note).HasMaxLength(300);

                // NULL = khách lẻ
                e.HasOne(x => x.Customer)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(x => x.Staff)
                    .WithMany(emp => emp.Orders)
                    .HasForeignKey(x => x.StaffId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Promotion)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(x => x.PromotionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private static void ConfigureOrderItems(ModelBuilder b)
        {
            b.Entity<OrderItem>(e =>
            {
                e.ToTable("OrderItems");
                e.HasKey(x => x.Id);
                e.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
                e.Property(x => x.Size).IsRequired().HasMaxLength(10);
                e.Property(x => x.Color).IsRequired().HasMaxLength(50);
                e.Property(x => x.UnitPrice).HasColumnType("decimal(15,2)");
                e.Property(x => x.LineTotal).HasColumnType("decimal(15,2)");

                // CASCADE DELETE theo Order
                e.HasOne(x => x.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Variant)
                    .WithMany(v => v.OrderItems)
                    .HasForeignKey(x => x.VariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureReturns(ModelBuilder b)
        {
            b.Entity<Return>(e =>
            {
                e.ToTable("Returns");
                e.HasKey(x => x.Id);
                e.Property(x => x.Reason).IsRequired().HasMaxLength(300);
                e.Property(x => x.ReturnType).HasConversion<string>().HasMaxLength(20).IsRequired();
                e.Property(x => x.RefundAmount).HasColumnType("decimal(15,2)");
                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

                e.HasOne(x => x.Order)
                    .WithMany(o => o.Returns)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Variant)
                    .WithMany(v => v.Returns)
                    .HasForeignKey(x => x.VariantId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Creator)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // ============================================================
        // 4. ACCOUNTING & AI
        // ============================================================

        private static void ConfigureExpenses(ModelBuilder b)
        {
            b.Entity<Expense>(e =>
            {
                e.ToTable("Expenses");
                e.HasKey(x => x.Id);
                e.Property(x => x.Category).IsRequired().HasMaxLength(100);
                e.Property(x => x.Amount).HasColumnType("decimal(15,2)");
                e.Property(x => x.Description).HasMaxLength(300);

                e.HasOne(x => x.Creator)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureSizeCharts(ModelBuilder b)
        {
            b.Entity<SizeChart>(e =>
            {
                e.ToTable("SizeCharts");
                e.HasKey(x => x.Id);
                e.Property(x => x.ProductType).HasConversion<string>().HasMaxLength(30).IsRequired();
                e.Property(x => x.Gender).HasConversion<string>().HasMaxLength(10).IsRequired();
                e.Property(x => x.Size).HasConversion<string>().HasMaxLength(10).IsRequired();

                e.Property(x => x.MinHeight).HasColumnType("decimal(5,1)");
                e.Property(x => x.MaxHeight).HasColumnType("decimal(5,1)");
                e.Property(x => x.MinWeight).HasColumnType("decimal(5,1)");
                e.Property(x => x.MaxWeight).HasColumnType("decimal(5,1)");
                e.Property(x => x.MinChest).HasColumnType("decimal(5,1)");
                e.Property(x => x.MaxChest).HasColumnType("decimal(5,1)");
                e.Property(x => x.MinWaist).HasColumnType("decimal(5,1)");
                e.Property(x => x.MaxWaist).HasColumnType("decimal(5,1)");
                e.Property(x => x.MinHip).HasColumnType("decimal(5,1)");
                e.Property(x => x.MaxHip).HasColumnType("decimal(5,1)");

                // INDEX: (productType, gender, size) UNIQUE
                e.HasIndex(x => new { x.ProductType, x.Gender, x.Size }).IsUnique();
            });
        }

        private static void ConfigureAILogs(ModelBuilder b)
        {
            b.Entity<AILog>(e =>
            {
                e.ToTable("AILogs");
                e.HasKey(x => x.Id);
                e.Property(x => x.Feature).HasConversion<string>().HasMaxLength(50).IsRequired();
                e.Property(x => x.InputData).HasColumnType("nvarchar(max)");
                e.Property(x => x.OutputData).HasColumnType("nvarchar(max)");
                e.Property(x => x.Model).HasMaxLength(50);
                e.Property(x => x.ErrorMessage).HasMaxLength(500);

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            // Lấy UserId từ JWT
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? userId = null;

            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var parsedId))
            {
                userId = parsedId;
            }

            foreach (var entry in ChangeTracker.Entries())
            {
                // ===== AUDIT =====
                if (entry.Entity is BaseEntity entity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedAt = now;
                        entity.CreatedBy = userId;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entity.UpdatedAt = now;
                        entity.UpdatedBy = userId;
                    }
                }

                // ===== SOFT DELETE =====
                if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable soft)
                {
                    entry.State = EntityState.Modified;
                    soft.IsDeleted = true;
                    soft.DeletedAt = now;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}