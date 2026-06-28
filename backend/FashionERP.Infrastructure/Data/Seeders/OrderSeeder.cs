namespace FashionERP.Infrastructure.Data.Seeders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Domain.Entities;
    using FashionERP.Domain.Enums;

    public static class OrderSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Orders.AnyAsync()) return;

            var random = new Random();
            var customers = await db.Customers.Select(c => c.Id).ToListAsync();
            var employees = await db.Employees.Select(e => e.Id).ToListAsync();
            var variants = await db.ProductVariants.Include(v => v.Product).ToListAsync();

            if (!variants.Any() || !customers.Any() || !employees.Any()) return;

            var orders = new List<Order>();
            var orderItems = new List<OrderItem>();
            var transactions = new List<InventoryTransaction>();

            int totalOrders = 150;
            var now = DateTime.UtcNow;

            for (int i = 1; i <= totalOrders; i++)
            {
                int daysAgo = random.Next(0, 90); // Đơn hàng rải rác 3 tháng qua
                var createdAt = now.AddDays(-daysAgo).AddHours(random.Next(-10, 10));
                var orderId = Guid.NewGuid();

                // ĐÃ SỬA: Dùng Enum OrderStatus thay vì string
                var statusRoll = random.Next(100);
                var status = OrderStatus.Completed;
                if (statusRoll > 95) status = OrderStatus.Cancelled;
                else if (statusRoll > 85) status = OrderStatus.Pending;

                var order = new Order
                {
                    Id = orderId,
                    OrderCode = $"ORD-{createdAt:yyyyMMdd}-{i:D3}",
                    CustomerId = customers[random.Next(customers.Count)],

                    // ĐÃ SỬA: Dùng StaffId theo đúng thiết kế trong Order.cs
                    StaffId = employees[random.Next(employees.Count)],

                    // ĐÃ SỬA: Dùng Enum PaymentMethod
                    PaymentMethod = random.Next(100) > 30 ? PaymentMethod.Transfer : PaymentMethod.Cash,
                    Status = status,
                    CreatedAt = createdAt,
                    CompletedAt = status == OrderStatus.Completed ? createdAt.AddHours(random.Next(1, 24)) : null,
                };

                int itemCount = random.Next(1, 4);
                decimal subtotal = 0;
                var pickedVariants = variants.OrderBy(x => random.Next()).Take(itemCount).ToList();

                foreach (var variant in pickedVariants)
                {
                    int qty = random.Next(1, 3);
                    decimal price = variant.Price ?? variant.Product.BasePrice;
                    decimal lineTotal = price * qty;
                    subtotal += lineTotal;

                    orderItems.Add(new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderId,
                        VariantId = variant.Id,
                        ProductName = variant.Product.Name,
                        Size = variant.Size.ToString(),
                        Color = variant.Color,
                        UnitPrice = price,
                        Quantity = qty,
                        LineTotal = lineTotal
                    });

                    // Quan trọng: Lưu lịch sử giao dịch kho cho AI Forecast đọc
                    if (status == OrderStatus.Completed)
                    {
                        transactions.Add(new InventoryTransaction
                        {
                            Id = Guid.NewGuid(),
                            VariantId = variant.Id,

                            // ĐÃ SỬA: Dùng Enum InventoryTransactionType
                            Type = InventoryTransactionType.EXPORT,

                            Quantity = -qty,
                            RefType = "Order",
                            RefId = orderId,
                            QuantityBefore = 100,
                            QuantityAfter = 100 - qty,
                            Note = $"Xuất kho bán hàng {order.OrderCode}",

                            // ĐÃ SỬA: Dùng StaffId thay vì EmployeeId
                            CreatedBy = order.StaffId,
                            CreatedAt = createdAt
                        });
                    }
                }

                order.Subtotal = subtotal;
                order.TaxAmount = subtotal * 0.08m; // VAT 8%
                order.FinalAmount = subtotal + order.TaxAmount;

                orders.Add(order);
            }

            await db.Orders.AddRangeAsync(orders);
            await db.OrderItems.AddRangeAsync(orderItems);
            await db.InventoryTransactions.AddRangeAsync(transactions);
            await db.SaveChangesAsync();

            Console.WriteLine($"[Seeder] Orders & InventoryTransactions: OK (150 đơn trải dài 3 tháng)");
        }
    }
}