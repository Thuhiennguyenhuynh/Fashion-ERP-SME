namespace FashionERP.Infrastructure.Data.Seeders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Domain.Entities;
    using FashionERP.Domain.Enums;

    /// <summary>
    /// Seed 3 phiếu đặt hàng NCC ở 3 trạng thái khác nhau để demo đủ luồng Procurement:
    ///  - PO_001: Completed + đã thanh toán 1 phần (còn nợ) → có CashTransaction EXPENSE
    ///  - PO_002: PartialReceived (nhận 1 phần hàng, còn nợ NCC)
    ///  - PO_003: Draft (chưa xác nhận, chưa nhận hàng)
    ///
    /// QUAN TRỌNG: Vì seeder KHÔNG đi qua PurchaseOrderService, nên phải tự mô phỏng
    /// đúng logic ReceiveItemsAsync (cộng Inventory.Quantity + tính lại AvgCost +
    /// ghi InventoryTransaction) và PaySupplierAsync (tạo CashTransaction EXPENSE)
    /// để dữ liệu nhất quán với khi dùng API thật.
    /// </summary>
    public static class PurchaseOrderSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.PurchaseOrders.AnyAsync()) return;

            var variants = await db.ProductVariants
                .Include(v => v.Inventory)
                .Include(v => v.Product)
                .Where(v => v.Inventory != null)
                .Take(4)
                .ToListAsync();

            if (variants.Count < 2) return; // cần VariantInventorySeeder chạy trước

            var now = DateTime.UtcNow;

            // ============================================================
            // PO_001: Đặt 2 dòng, NHẬN ĐỦ → Completed, đã thanh toán 1 phần
            // ============================================================
            var po1Items = new List<PurchaseOrderItem>
            {
                MakeItem(variants[0], orderedQty: 30, unitCost: 90_000),
                MakeItem(variants[1], orderedQty: 20, unitCost: 95_000)
            };
            var po1Total = po1Items.Sum(i => i.LineTotal);

            var po1 = new PurchaseOrder
            {
                Id = SeedIds.PO_001,
                PoCode = $"PO-{now:yyyyMMdd}-001",
                SupplierId = SeedIds.Sup_VinaTextile,
                Status = PurchaseOrderStatus.Completed,
                TotalAmount = po1Total,
                PaidAmount = 0,
                DebtAmount = 0, // tính lại sau khi "nhận hàng"
                ExpectedDate = now.AddDays(-10),
                ReceivedDate = now.AddDays(-7),
                Note = "Đặt hàng bổ sung tồn áo thun + quần jean",
                Items = po1Items
            };
            db.PurchaseOrders.Add(po1);
            await db.SaveChangesAsync();

            await ReceiveAllAndUpdateStock(db, po1, po1Items, refNote: "Nhận hàng PO-001");

            // Thanh toán 1 phần (60% công nợ) → sinh CashTransaction EXPENSE
            var payAmount1 = Math.Round(po1.DebtAmount * 0.6m, 0);
            await PaySupplier(db, po1, SeedIds.Sup_VinaTextile, payAmount1, "Thanh toán đợt 1 cho PO-001");

            // ============================================================
            // PO_002: Đặt 2 dòng, chỉ NHẬN 1 PHẦN → PartialReceived
            // ============================================================
            var po2Items = new List<PurchaseOrderItem>
            {
                MakeItem(variants[2 % variants.Count], orderedQty: 40, unitCost: 110_000),
                MakeItem(variants[3 % variants.Count], orderedQty: 25, unitCost: 105_000)
            };
            var po2Total = po2Items.Sum(i => i.LineTotal);

            var po2 = new PurchaseOrder
            {
                Id = SeedIds.PO_002,
                PoCode = $"PO-{now:yyyyMMdd}-002",
                SupplierId = SeedIds.Sup_HanoiFabric,
                Status = PurchaseOrderStatus.Ordered,
                TotalAmount = po2Total,
                PaidAmount = 0,
                DebtAmount = 0,
                ExpectedDate = now.AddDays(-3),
                ReceivedDate = null,
                Note = "Đặt vải sơ mi + áo dài tay cho mùa sau",
                Items = po2Items
            };
            db.PurchaseOrders.Add(po2);
            await db.SaveChangesAsync();

            // Chỉ nhận 50% mỗi dòng → PartialReceived
            await ReceivePartialAndUpdateStock(db, po2, po2Items, percent: 0.5m, refNote: "Nhận 1 phần hàng PO-002");

            // ============================================================
            // PO_003: Draft — chưa xác nhận, chưa nhận hàng, chưa có công nợ
            // ============================================================
            var po3Items = new List<PurchaseOrderItem>
            {
                MakeItem(variants[0], orderedQty: 50, unitCost: 88_000)
            };
            var po3 = new PurchaseOrder
            {
                Id = SeedIds.PO_003,
                PoCode = $"PO-{now:yyyyMMdd}-003",
                SupplierId = SeedIds.Sup_SaigonDenim,
                Status = PurchaseOrderStatus.Draft,
                TotalAmount = po3Items.Sum(i => i.LineTotal),
                PaidAmount = 0,
                DebtAmount = 0,
                ExpectedDate = now.AddDays(7),
                Note = "Dự kiến đặt thêm denim cho tháng sau",
                Items = po3Items
            };
            db.PurchaseOrders.Add(po3);
            await db.SaveChangesAsync();

            Console.WriteLine("[Seeder] Suppliers + PurchaseOrders: OK");
        }

        private static PurchaseOrderItem MakeItem(ProductVariant v, int orderedQty, decimal unitCost) => new()
        {
            VariantId = v.Id,
            ProductName = v.Product.Name,
            Size = v.Size.ToString(),
            Color = v.Color,
            OrderedQty = orderedQty,
            ReceivedQty = 0,
            UnitCost = unitCost,
            LineTotal = orderedQty * unitCost
        };

        /// <summary>Mô phỏng nhận đủ 100% hàng — giống PurchaseOrderService.ReceiveItemsAsync</summary>
        private static async Task ReceiveAllAndUpdateStock(
            AppDbContext db, PurchaseOrder po, List<PurchaseOrderItem> items, string refNote)
        {
            decimal receivedValue = 0;
            foreach (var item in items)
            {
                item.ReceivedQty = item.OrderedQty;
                receivedValue += item.OrderedQty * item.UnitCost;
                await ApplyStockIn(db, item.VariantId, item.OrderedQty, item.UnitCost, po.Id, refNote);
            }

            po.DebtAmount += receivedValue;
            po.Status = PurchaseOrderStatus.Completed;

            var supplier = await db.Suppliers.FindAsync(po.SupplierId);
            if (supplier != null) supplier.TotalDebt += receivedValue;

            await db.SaveChangesAsync();
        }

        /// <summary>Mô phỏng nhận 1 phần hàng (percent của OrderedQty)</summary>
        private static async Task ReceivePartialAndUpdateStock(
            AppDbContext db, PurchaseOrder po, List<PurchaseOrderItem> items, decimal percent, string refNote)
        {
            decimal receivedValue = 0;
            foreach (var item in items)
            {
                var qty = (int)Math.Floor(item.OrderedQty * percent);
                item.ReceivedQty = qty;
                receivedValue += qty * item.UnitCost;
                await ApplyStockIn(db, item.VariantId, qty, item.UnitCost, po.Id, refNote);
            }

            po.DebtAmount += receivedValue;
            po.Status = PurchaseOrderStatus.PartialReceived;

            var supplier = await db.Suppliers.FindAsync(po.SupplierId);
            if (supplier != null) supplier.TotalDebt += receivedValue;

            await db.SaveChangesAsync();
        }

        /// <summary>Cộng Inventory.Quantity + tính lại AvgCost (Moving Average) + ghi InventoryTransaction</summary>
        private static async Task ApplyStockIn(
            AppDbContext db, Guid variantId, int qty, decimal unitCost, Guid poId, string note)
        {
            if (qty <= 0) return;

            var inv = await db.Inventories.FirstOrDefaultAsync(i => i.VariantId == variantId);
            if (inv == null) return;

            var qtyBefore = inv.Quantity;
            inv.Quantity += qty;
            inv.AvgCost = (inv.AvgCost * qtyBefore + unitCost * qty) / inv.Quantity;
            inv.LastImportDate = DateTime.UtcNow;

            db.InventoryTransactions.Add(new InventoryTransaction
            {
                VariantId = variantId,
                Type = InventoryTransactionType.IMPORT,
                Quantity = qty,
                UnitCost = unitCost,
                RefType = "PurchaseOrder",
                RefId = poId,
                QuantityBefore = qtyBefore,
                QuantityAfter = inv.Quantity,
                Note = note
            });
        }

        /// <summary>Mô phỏng PaySupplierAsync — giảm công nợ + tạo CashTransaction EXPENSE</summary>
        private static async Task PaySupplier(
            AppDbContext db, PurchaseOrder po, Guid supplierId, decimal amount, string note)
        {
            if (amount <= 0) return;

            po.PaidAmount += amount;
            po.DebtAmount -= amount;

            var supplier = await db.Suppliers.FindAsync(supplierId);
            if (supplier != null) supplier.TotalDebt -= amount;

            var lastBalance = await db.CashTransactions
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.CreatedAt)
                .Select(t => t.BalanceAfter)
                .FirstOrDefaultAsync();

            db.CashTransactions.Add(new CashTransaction
            {
                Type = CashTransactionType.EXPENSE,
                Category = "Thanh toán NCC",
                Amount = amount,
                Note = note,
                RefType = "PurchaseOrder",
                RefId = po.Id,
                BalanceAfter = lastBalance - amount,
                TransactionDate = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}
