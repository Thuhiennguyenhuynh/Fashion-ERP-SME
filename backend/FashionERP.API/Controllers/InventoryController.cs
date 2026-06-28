using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.DTOs.Inventory;
using FashionERP.Application.Interfaces;
using FashionERP.Infrastructure.Data;

namespace FashionERP.API.Controllers
{
    [Authorize]
    public class InventoryController : BaseController
    {
        private readonly IInventoryService _inventoryService;
        private readonly AppDbContext _db;

        public InventoryController(IInventoryService inventoryService, AppDbContext db)
        {
            _inventoryService = inventoryService;
            _db = db;
        }

        /// <summary>Xem tồn kho tất cả biến thể (có thể lọc hàng sắp hết)</summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Warehouse")]
        public async Task<IActionResult> GetAll([FromQuery] bool? lowStockOnly)
        {
            var result = await _inventoryService.GetAllAsync(lowStockOnly);
            return Ok(result);
        }

        /// <summary>Xem tồn kho theo variantId</summary>
        [HttpGet("variant/{variantId:guid}")]
        public async Task<IActionResult> GetByVariant(Guid variantId)
        {
            var result = await _inventoryService.GetByVariantIdAsync(variantId);
            return Ok(result);
        }

        /// <summary>Nhập kho thủ công</summary>
        [HttpPost("import")]
        [Authorize(Roles = "Admin,Manager,Warehouse")]
        public async Task<IActionResult> Import([FromBody] ImportStockRequestDto request)
        {
            await _inventoryService.ImportStockAsync(request, CurrentUserId);
            return Ok<object>(null!, "Nhập kho thành công");
        }

        /// <summary>Điều chỉnh tồn kho (kiểm kê)</summary>
        [HttpPost("adjust")]
        [Authorize(Roles = "Admin,Manager,Warehouse")]
        public async Task<IActionResult> Adjust([FromBody] AdjustStockRequestDto request)
        {
            await _inventoryService.AdjustStockAsync(request, CurrentUserId);
            return Ok<object>(null!, "Điều chỉnh tồn kho thành công");
        }

        /// <summary>Lịch sử giao dịch kho — filter tuỳ chọn</summary>
        [HttpGet("transactions")]
        [Authorize(Roles = "Admin,Manager,Warehouse")]
        public async Task<IActionResult> GetAllTransactions(
            [FromQuery] Guid? variantId,
            [FromQuery] string? type,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _db.InventoryTransactions
                .Include(t => t.Variant)
                    .ThenInclude(v => v.Product)
                .AsQueryable();

            if (variantId.HasValue)
                query = query.Where(t => t.VariantId == variantId.Value);
            if (!string.IsNullOrEmpty(type))
                query = query.Where(t => t.Type.ToString() == type);
            if (from.HasValue)
                query = query.Where(t => t.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(t => t.CreatedAt <= to.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    Type = t.Type.ToString(),
                    t.Quantity,
                    t.UnitCost,
                    t.QuantityBefore,
                    t.QuantityAfter,
                    t.Note,
                    t.CreatedAt,
                    t.CreatedBy,
                    Variant = new
                    {
                        t.Variant.Sku,
                        t.Variant.Size,
                        t.Variant.Color,
                        ProductName = t.Variant.Product.Name
                    }
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }
        [HttpGet]
[Authorize(Roles = "Admin,Manager,Warehouse")]
public async Task<IActionResult> GetAll(
    [FromQuery] bool? lowStockOnly,
    [FromQuery] Guid? productId,
    [FromQuery] string? keyword,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var result = await _inventoryService.GetAllAsync(lowStockOnly, productId, keyword, page, pageSize);
    return Ok(result);
}
        /// <summary>Lịch sử giao dịch kho theo variantId</summary>
        [HttpGet("transactions/{variantId:guid}")]
        [Authorize(Roles = "Admin,Manager,Warehouse")]
        public async Task<IActionResult> GetTransactions(
            Guid variantId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _db.InventoryTransactions
                .Where(t => t.VariantId == variantId)
                .OrderByDescending(t => t.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    Type = t.Type.ToString(),
                    t.Quantity,
                    t.UnitCost,
                    t.RefType,
                    t.QuantityBefore,
                    t.QuantityAfter,
                    t.Note,
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }
    }
}
