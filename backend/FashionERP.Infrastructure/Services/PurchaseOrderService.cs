using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Procurement;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly AppDbContext _db;

        public PurchaseOrderService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<PurchaseOrderResponseDto>> GetAllAsync(
            string? status, Guid? supplierId, DateTime? from, DateTime? to, int page, int pageSize)
        {
            var query = _db.PurchaseOrders
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<PurchaseOrderStatus>(status, out var parsedStatus))
                query = query.Where(p => p.Status == parsedStatus);

            if (supplierId.HasValue)
                query = query.Where(p => p.SupplierId == supplierId.Value);

            if (from.HasValue)
                query = query.Where(p => p.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(p => p.CreatedAt <= to.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PurchaseOrderResponseDto
                {
                    Id = p.Id,
                    PoCode = p.PoCode,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.Name,
                    Status = p.Status.ToString(),
                    TotalAmount = p.TotalAmount,
                    PaidAmount = p.PaidAmount,
                    DebtAmount = p.DebtAmount,
                    ExpectedDate = p.ExpectedDate,
                    ReceivedDate = p.ReceivedDate,
                    Note = p.Note,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy
                })
                .ToListAsync();

            return new PagedResult<PurchaseOrderResponseDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PurchaseOrderResponseDto> GetByIdAsync(Guid id)
        {
            var po = await _db.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException("Phiếu đặt hàng", id);

            return new PurchaseOrderResponseDto
            {
                Id = po.Id,
                PoCode = po.PoCode,
                SupplierId = po.SupplierId,
                SupplierName = po.Supplier.Name,
                Status = po.Status.ToString(),
                TotalAmount = po.TotalAmount,
                PaidAmount = po.PaidAmount,
                DebtAmount = po.DebtAmount,
                ExpectedDate = po.ExpectedDate,
                ReceivedDate = po.ReceivedDate,
                Note = po.Note,
                CreatedAt = po.CreatedAt,
                CreatedBy = po.CreatedBy,
                Items = po.Items.Select(i => new PurchaseOrderItemResponseDto
                {
                    Id = i.Id,
                    VariantId = i.VariantId,
                    ProductName = i.ProductName,
                    Size = i.Size,
                    Color = i.Color,
                    OrderedQty = i.OrderedQty,
                    ReceivedQty = i.ReceivedQty,
                    UnitCost = i.UnitCost,
                    LineTotal = i.LineTotal
                }).ToList()
            };
        }

        public async Task<PurchaseOrderResponseDto> CreateAsync(CreatePurchaseOrderRequestDto request, Guid createdBy)
        {
            if (!await _db.Suppliers.AnyAsync(s => s.Id == request.SupplierId))
                throw new NotFoundException("Nhà cung cấp", request.SupplierId);

            if (request.Items == null || request.Items.Count == 0)
                throw new BusinessException("Phiếu đặt hàng phải có ít nhất 1 sản phẩm");

            var today = DateTime.UtcNow;
            var dateStr = today.ToString("yyyyMMdd");
            var countToday = await _db.PurchaseOrders.CountAsync(o => o.PoCode.StartsWith($"PO-{dateStr}-"));
            var poCode = $"PO-{dateStr}-{(countToday + 1):D3}";

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var po = new PurchaseOrder
                {
                    PoCode = poCode,
                    SupplierId = request.SupplierId,
                    Status = PurchaseOrderStatus.Draft,
                    ExpectedDate = request.ExpectedDate,
                    Note = request.Note,
                    TotalAmount = 0, // Tính sau
                    DebtAmount = 0,
                    PaidAmount = 0
                };
                _db.PurchaseOrders.Add(po);
                await _db.SaveChangesAsync();

                decimal totalAmount = 0;
                foreach (var itemDto in request.Items)
                {
                    if (itemDto.OrderedQty <= 0 || itemDto.UnitCost < 0)
                        throw new BusinessException("Số lượng đặt phải > 0 và Đơn giá phải >= 0");

                    var variant = await _db.ProductVariants
                        .Include(v => v.Product)
                        .FirstOrDefaultAsync(v => v.Id == itemDto.VariantId)
                        ?? throw new NotFoundException("Biến thể", itemDto.VariantId);

                    var lineTotal = itemDto.OrderedQty * itemDto.UnitCost;
                    totalAmount += lineTotal;

                    _db.PurchaseOrderItems.Add(new PurchaseOrderItem
                    {
                        PurchaseOrderId = po.Id,
                        VariantId = variant.Id,
                        ProductName = variant.Product.Name,
                        Size = variant.Size.ToString(),
                        Color = variant.Color,
                        OrderedQty = itemDto.OrderedQty,
                        ReceivedQty = 0,
                        UnitCost = itemDto.UnitCost,
                        LineTotal = lineTotal
                    });
                }

                po.TotalAmount = totalAmount;
                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return await GetByIdAsync(po.Id);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task ConfirmOrderAsync(Guid id)
        {
            var po = await _db.PurchaseOrders.FindAsync(id)
                ?? throw new NotFoundException("Phiếu đặt hàng", id);

            if (po.Status != PurchaseOrderStatus.Draft)
                throw new BusinessException("Chỉ có thể xác nhận đặt hàng với phiếu ở trạng thái Nháp (Draft)");

            po.Status = PurchaseOrderStatus.Ordered;
            await _db.SaveChangesAsync();
        }

        public async Task<PurchaseOrderResponseDto> ReceiveItemsAsync(Guid id, ReceivePurchaseOrderRequestDto request, Guid receivedBy)
        {
            var po = await _db.PurchaseOrders
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException("Phiếu đặt hàng", id);

            if (po.Status != PurchaseOrderStatus.Ordered && po.Status != PurchaseOrderStatus.PartialReceived)
                throw new BusinessException("Chỉ nhận hàng được khi phiếu ở trạng thái Đã đặt (Ordered) hoặc Nhận một phần");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                decimal receivedValueThisTime = 0;

                foreach (var reqItem in request.Items)
                {
                    if (reqItem.ReceivedQtyThisTime <= 0) continue;

                    var poItem = po.Items.FirstOrDefault(i => i.Id == reqItem.PurchaseOrderItemId)
                        ?? throw new BusinessException($"Không tìm thấy dòng sản phẩm {reqItem.PurchaseOrderItemId} trong phiếu");

                    if (poItem.ReceivedQty + reqItem.ReceivedQtyThisTime > poItem.OrderedQty)
                        throw new BusinessException($"Sản phẩm '{poItem.ProductName}' nhận vượt số lượng đặt");

                    poItem.ReceivedQty += reqItem.ReceivedQtyThisTime;
                    receivedValueThisTime += reqItem.ReceivedQtyThisTime * poItem.UnitCost;

                    // 1. Cập nhật Inventory (Tồn kho + Trung bình giá)
                    var inv = await _db.Inventories.FirstOrDefaultAsync(i => i.VariantId == poItem.VariantId);
                    if (inv != null)
                    {
                        var qBefore = inv.Quantity;
                        inv.Quantity += reqItem.ReceivedQtyThisTime;

                        // Công thức tính trung bình giá (Moving Average)
                        inv.AvgCost = ((inv.AvgCost * qBefore) + (poItem.UnitCost * reqItem.ReceivedQtyThisTime)) / inv.Quantity;
                        inv.LastImportDate = DateTime.UtcNow;

                        // 2. Ghi Audit log kho
                        _db.InventoryTransactions.Add(new InventoryTransaction
                        {
                            VariantId = poItem.VariantId,
                            Type = InventoryTransactionType.IMPORT,
                            Quantity = reqItem.ReceivedQtyThisTime,
                            UnitCost = poItem.UnitCost,
                            RefType = "PurchaseOrder",
                            RefId = po.Id,
                            QuantityBefore = qBefore,
                            QuantityAfter = inv.Quantity,
                            Note = $"Nhận hàng từ PO {po.PoCode}",
                            CreatedBy = receivedBy
                        });
                    }
                }

                // 3. Cập nhật công nợ
                po.DebtAmount += receivedValueThisTime;

                var supplier = await _db.Suppliers.FindAsync(po.SupplierId);
                if (supplier != null)
                    supplier.TotalDebt += receivedValueThisTime;

                // 4. Kiểm tra trạng thái hoàn tất
                bool isAllReceived = po.Items.All(i => i.ReceivedQty == i.OrderedQty);
                if (isAllReceived)
                {
                    po.Status = PurchaseOrderStatus.Completed;
                    po.ReceivedDate = DateTime.UtcNow;
                }
                else
                {
                    po.Status = PurchaseOrderStatus.PartialReceived;
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return await GetByIdAsync(po.Id);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task PaySupplierAsync(Guid id, PayPurchaseOrderRequestDto request, Guid paidBy)
        {
            if (request.Amount <= 0)
                throw new BusinessException("Số tiền thanh toán phải > 0");

            var po = await _db.PurchaseOrders.FindAsync(id)
                ?? throw new NotFoundException("Phiếu đặt hàng", id);

            if (request.Amount > po.DebtAmount)
                throw new BusinessException($"Số tiền thanh toán ({request.Amount}) vượt quá công nợ hiện tại của phiếu ({po.DebtAmount})");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                po.PaidAmount += request.Amount;
                po.DebtAmount -= request.Amount;

                var supplier = await _db.Suppliers.FindAsync(po.SupplierId);
                if (supplier != null)
                    supplier.TotalDebt -= request.Amount;

                // Note: Module Finance (CashTransaction) sẽ được gọi để insert ở đây trong tương lai

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task CancelAsync(Guid id)
        {
            var po = await _db.PurchaseOrders.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException("Phiếu đặt hàng", id);

            if (po.Status != PurchaseOrderStatus.Draft && po.Status != PurchaseOrderStatus.Ordered)
                throw new BusinessException("Chỉ có thể hủy phiếu khi chưa nhận hàng (trạng thái Nháp hoặc Đã đặt)");

            if (po.Items.Any(i => i.ReceivedQty > 0))
                throw new BusinessException("Không thể hủy phiếu vì đã có sản phẩm được nhận");

            po.Status = PurchaseOrderStatus.Cancelled;
            await _db.SaveChangesAsync();
        }
    }
}