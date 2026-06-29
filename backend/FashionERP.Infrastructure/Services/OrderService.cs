using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Order;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICashTransactionService _cashService;

        public OrderService(AppDbContext db, IMapper mapper, ICashTransactionService cashService)
        {
            _db = db;
            _mapper = mapper;
            _cashService = cashService;
        }

        private IQueryable<Order> BaseQuery() =>
            _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.Items)
                .Include(o => o.Promotion);

        // ─── GET BY ID ────────────────────────────────────────
        public async Task<OrderResponseDto> GetByIdAsync(Guid id)
        {
            var order = await BaseQuery().FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new NotFoundException("Đơn hàng", id);
            return _mapper.Map<OrderResponseDto>(order);
        }

        // ─── CREATE ORDER ─────────────────────────────────────
        public async Task<OrderResponseDto> CreateAsync(CreateOrderRequestDto request, Guid staffId)
        {
            if (!await _db.Employees.AnyAsync(e => e.Id == staffId))
                throw new NotFoundException("Nhân viên", staffId);

            if (request.CustomerId.HasValue &&
                !await _db.Customers.AnyAsync(c => c.Id == request.CustomerId.Value))
                throw new NotFoundException("Khách hàng", request.CustomerId.Value);

            if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, out var payMethod))
                throw new AppException("Phương thức thanh toán không hợp lệ");

            var today = DateTime.UtcNow;
            var dateStr = today.ToString("yyyyMMdd");
            var countToday = await _db.Orders
                .CountAsync(o => o.OrderCode.StartsWith($"ORD-{dateStr}-"));
            var orderCode = $"ORD-{dateStr}-{(countToday + 1):D3}";

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                decimal subtotal = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in request.Items)
                {
                    var variant = await _db.ProductVariants
                        .Include(v => v.Product)
                        .Include(v => v.Inventory)
                        .FirstOrDefaultAsync(v => v.Id == item.VariantId)
                        ?? throw new NotFoundException("Biến thể sản phẩm", item.VariantId);

                    if (!variant.IsActive)
                        throw new BusinessException($"Sản phẩm '{variant.Product.Name} - {variant.Size} - {variant.Color}' hiện không còn bán");

                    if (variant.Inventory == null || variant.Inventory.Quantity < item.Quantity)
                        throw new BusinessException(
                            $"Sản phẩm '{variant.Product.Name} - {variant.Size} - {variant.Color}' " +
                            $"không đủ tồn kho. Hiện còn: {variant.Inventory?.Quantity ?? 0}, yêu cầu: {item.Quantity}");

                    var unitPrice = variant.Price ?? variant.Product.BasePrice;
                    var lineTotal = unitPrice * item.Quantity;
                    subtotal += lineTotal;

                    // KHAI BÁO BIẾN GIÁ VỐN Ở ĐÂY ĐỂ TRÁNH LỖI CS0103
                    decimal unitCostSnapshot = variant.Inventory?.AvgCost ?? 0m;

                    orderItems.Add(new OrderItem
                    {
                        VariantId = item.VariantId,
                        ProductName = variant.Product.Name,
                        Size = variant.Size.ToString(),
                        Color = variant.Color,
                        UnitPrice = unitPrice,
                        Quantity = item.Quantity,
                        LineTotal = lineTotal,
                        UnitCostSnapshot = unitCostSnapshot // Biến đã được khai báo và gán giá trị
                    });

                    var qBefore = variant.Inventory.Quantity;
                    variant.Inventory.Quantity -= item.Quantity;
                    variant.Inventory.UpdatedAt = DateTime.UtcNow;

                    _db.InventoryTransactions.Add(new InventoryTransaction
                    {
                        VariantId = item.VariantId,
                        Type = InventoryTransactionType.EXPORT,
                        Quantity = -item.Quantity,
                        RefType = "Order",
                        QuantityBefore = qBefore,
                        QuantityAfter = variant.Inventory.Quantity,
                        Note = $"Xuất kho theo đơn hàng {orderCode}",
                        CreatedBy = staffId
                    });
                }

                decimal discountAmount = 0;
                Guid? promotionId = null;
                string? promotionCodeSnapshot = null;

                if (!string.IsNullOrEmpty(request.PromotionCode))
                {
                    var promo = await _db.Promotions.FirstOrDefaultAsync(p =>
                        p.Code == request.PromotionCode.Trim().ToUpper() &&
                        p.IsActive &&
                        p.StartDate <= today &&
                        p.EndDate >= today);

                    if (promo == null)
                        throw new BusinessException("Mã khuyến mãi không hợp lệ hoặc đã hết hạn");

                    if (promo.UsageLimit.HasValue && promo.UsedCount >= promo.UsageLimit.Value)
                        throw new BusinessException("Mã khuyến mãi đã đạt giới hạn lượt sử dụng");

                    if (subtotal < promo.MinOrderValue)
                        throw new BusinessException(
                            $"Đơn hàng tối thiểu {promo.MinOrderValue:N0} VNĐ mới được áp dụng mã này");

                    discountAmount = promo.Type == PromotionType.Percent
                        ? Math.Min(subtotal * promo.DiscountValue / 100,
                                   promo.MaxDiscount ?? decimal.MaxValue)
                        : promo.DiscountValue;

                    promo.UsedCount++;
                    promo.UpdatedAt = DateTime.UtcNow;
                    promotionId = promo.Id;
                    promotionCodeSnapshot = promo.Code;
                }

                const decimal vatRate = 0.10m;
                var taxAmount = (subtotal - discountAmount) * vatRate;
                var finalAmount = subtotal - discountAmount + taxAmount;

                var order = new Order
                {
                    OrderCode = orderCode,
                    CustomerId = request.CustomerId,
                    StaffId = staffId,
                    Subtotal = subtotal,
                    DiscountAmount = discountAmount,
                    TaxAmount = taxAmount,
                    FinalAmount = finalAmount,
                    PaymentMethod = payMethod,
                    PromotionId = promotionId,
                    PromotionCode = promotionCodeSnapshot,
                    Note = request.Note?.Trim(),
                    Status = OrderStatus.Completed,
                    CompletedAt = DateTime.UtcNow
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();
                // Tự động ghi nhận doanh thu vào Sổ quỹ
                await _cashService.RecordAsync(
                    CashTransactionType.INCOME, "Bán hàng", order.FinalAmount,
                    refType: "Order", refId: order.Id, note: $"Thu tiền đơn {order.OrderCode}", createdBy: null);

                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                    _db.OrderItems.Add(item);
                }

                if (request.CustomerId.HasValue)
                {
                    var customer = await _db.Customers.FindAsync(request.CustomerId.Value);
                    if (customer != null)
                    {
                        customer.TotalSpent += finalAmount;
                        customer.TotalOrders++;
                        customer.MemberLevel = CustomerService.CalcMemberLevel(customer.TotalSpent);
                        customer.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return await GetByIdAsync(order.Id);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ─── CANCEL ───────────────────────────────────────────
        public async Task CancelAsync(Guid id)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new NotFoundException("Đơn hàng", id);

            if (order.Status == OrderStatus.Cancelled)
                throw new BusinessException("Đơn hàng này đã được hủy trước đó");

            if (order.Status == OrderStatus.Returned || order.Status == OrderStatus.PartiallyReturned)
                throw new BusinessException("Không thể hủy đơn hàng đã đổi trả (toàn bộ hoặc một phần)");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in order.Items)
                {
                    var inv = await _db.Inventories
                        .FirstOrDefaultAsync(i => i.VariantId == item.VariantId);
                    if (inv != null)
                    {
                        var qBefore = inv.Quantity;
                        inv.Quantity += item.Quantity;
                        inv.UpdatedAt = DateTime.UtcNow;

                        _db.InventoryTransactions.Add(new InventoryTransaction
                        {
                            VariantId = item.VariantId,
                            Type = InventoryTransactionType.IMPORT,
                            Quantity = item.Quantity,
                            RefType = "OrderCancelled",
                            RefId = order.Id,
                            QuantityBefore = qBefore,
                            QuantityAfter = inv.Quantity,
                            Note = $"Hoàn kho do hủy đơn {order.OrderCode}",
                            CreatedBy = order.StaffId
                        });
                    }
                }

                if (order.CustomerId.HasValue)
                {
                    var customer = await _db.Customers.FindAsync(order.CustomerId.Value);
                    if (customer != null)
                    {
                        customer.TotalSpent = Math.Max(0, customer.TotalSpent - order.FinalAmount);
                        customer.TotalOrders = Math.Max(0, customer.TotalOrders - 1);
                        customer.MemberLevel = CustomerService.CalcMemberLevel(customer.TotalSpent);
                        customer.UpdatedAt = DateTime.UtcNow;
                    }
                }

                if (order.PromotionId.HasValue)
                {
                    var promo = await _db.Promotions.FindAsync(order.PromotionId.Value);
                    if (promo != null)
                    {
                        promo.UsedCount = Math.Max(0, promo.UsedCount - 1);
                        promo.UpdatedAt = DateTime.UtcNow;
                    }
                }

                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ─── COMPLETE ─────────────────────────────────────────
        public async Task CompleteAsync(Guid id)
        {
            var order = await _db.Orders.FindAsync(id)
                ?? throw new NotFoundException("Đơn hàng", id);

            if (order.Status != OrderStatus.Pending)
                throw new BusinessException("Chỉ có thể hoàn tất đơn hàng đang ở trạng thái Pending");

            order.Status = OrderStatus.Completed;
            order.CompletedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // ─── CREATE RETURN ────────────────────────────────────
        public async Task<OrderResponseDto> CreateReturnAsync(
            CreateReturnRequestDto request, Guid createdBy)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId)
                ?? throw new NotFoundException("Đơn hàng", request.OrderId);

            if (order.Status != OrderStatus.Completed && order.Status != OrderStatus.PartiallyReturned)
                throw new BusinessException("Chỉ có thể đổi trả đơn hàng đã hoàn thành (hoặc đang trả một phần)");

            var orderItem = order.Items.FirstOrDefault(i => i.VariantId == request.VariantId)
                ?? throw new BusinessException("Sản phẩm này không có trong đơn hàng cần đổi trả");

            var alreadyReturned = await _db.Returns
                .Where(r => r.OrderId == request.OrderId && r.VariantId == request.VariantId)
                .SumAsync(r => r.Quantity);

            if (alreadyReturned + request.Quantity > orderItem.Quantity)
                throw new BusinessException(
                    $"Số lượng trả ({request.Quantity}) vượt quá số lượng có thể trả " +
                    $"(đã mua: {orderItem.Quantity}, đã trả trước đó: {alreadyReturned})");

            if (!Enum.TryParse<ReturnType>(request.ReturnType, out var returnType))
                throw new AppException("Hình thức đổi trả không hợp lệ");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var ret = new Return
                {
                    OrderId = request.OrderId,
                    VariantId = request.VariantId,
                    Quantity = request.Quantity,
                    Reason = request.Reason.Trim(),
                    ReturnType = returnType,
                    RefundAmount = request.RefundAmount,
                    Status = ReturnStatus.Completed,
                    CreatedBy = createdBy
                };
                _db.Returns.Add(ret);

                var inv = await _db.Inventories
                    .FirstOrDefaultAsync(i => i.VariantId == request.VariantId);
                if (inv != null)
                {
                    var qBefore = inv.Quantity;
                    inv.Quantity += request.Quantity;
                    inv.UpdatedAt = DateTime.UtcNow;

                    _db.InventoryTransactions.Add(new InventoryTransaction
                    {
                        VariantId = request.VariantId,
                        Type = InventoryTransactionType.RETURN,
                        Quantity = request.Quantity,
                        RefType = "Return",
                        RefId = order.Id,
                        QuantityBefore = qBefore,
                        QuantityAfter = inv.Quantity,
                        Note = $"Hoàn kho do đổi trả đơn {order.OrderCode}",
                        CreatedBy = createdBy
                    });
                }

                await _db.SaveChangesAsync();

                var totalOrderedQuantity = order.Items.Sum(i => i.Quantity);
                var totalReturnedQuantity = await _db.Returns
                    .Where(r => r.OrderId == order.Id)
                    .SumAsync(r => r.Quantity);

                order.Status = totalReturnedQuantity >= totalOrderedQuantity
                    ? OrderStatus.Returned
                    : OrderStatus.PartiallyReturned;
                order.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return await GetByIdAsync(order.Id);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ─── GET ALL (PAGED) ──────────────────────────────────
        public async Task<PagedResult<OrderResponseDto>> GetAllAsync(
            string? status, DateTime? from, DateTime? to,
            Guid? staffId, int page, int pageSize)
        {
            var query = _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var parsedStatus))
                query = query.Where(o => o.Status == parsedStatus);

            if (from.HasValue)
                query = query.Where(o => o.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(o => o.CreatedAt <= to.Value);
            if (staffId.HasValue)
                query = query.Where(o => o.StaffId == staffId.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => _mapper.Map<OrderResponseDto>(o))
                .ToListAsync();

            return new PagedResult<OrderResponseDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}