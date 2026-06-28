using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Inventory;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public InventoryService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        private IQueryable<Inventory> BaseQuery() =>
            _db.Inventories
                .Include(i => i.Variant).ThenInclude(v => v.Product);

        // ─── GET BY VARIANT ───────────────────────────────────
        public async Task<InventoryResponseDto> GetByVariantIdAsync(Guid variantId)
        {
            var inv = await BaseQuery().FirstOrDefaultAsync(i => i.VariantId == variantId)
                ?? throw new NotFoundException("Tồn kho", variantId);
            return _mapper.Map<InventoryResponseDto>(inv);
        }

        // ─── IMPORT STOCK ─────────────────────────────────────
        public async Task ImportStockAsync(ImportStockRequestDto request, Guid createdBy)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var inv = await _db.Inventories
                    .FirstOrDefaultAsync(i => i.VariantId == request.VariantId)
                    ?? throw new NotFoundException("Tồn kho của biến thể", request.VariantId);

                var qBefore = inv.Quantity;
                inv.Quantity += request.Quantity;

                if (request.UnitCost > 0)
                {
                    var totalValue = (inv.AvgCost * qBefore) + (request.UnitCost * request.Quantity);
                    inv.AvgCost = totalValue / inv.Quantity;
                }

                inv.LastImportDate = DateTime.UtcNow;
                inv.UpdatedAt = DateTime.UtcNow;

                var txRecord = new InventoryTransaction
                {
                    VariantId = request.VariantId,
                    Type = InventoryTransactionType.IMPORT,
                    Quantity = request.Quantity,
                    UnitCost = request.UnitCost,
                    RefType = "ManualImport",
                    QuantityBefore = qBefore,
                    QuantityAfter = inv.Quantity,
                    Note = request.Note,
                    CreatedBy = createdBy
                };
                _db.InventoryTransactions.Add(txRecord);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ─── ADJUST STOCK ─────────────────────────────────────
        public async Task AdjustStockAsync(AdjustStockRequestDto request, Guid createdBy)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var inv = await _db.Inventories
                    .FirstOrDefaultAsync(i => i.VariantId == request.VariantId)
                    ?? throw new NotFoundException("Tồn kho của biến thể", request.VariantId);

                var qBefore = inv.Quantity;
                var diff = request.NewQuantity - qBefore;

                if (diff == 0)
                    throw new BusinessException("Số lượng điều chỉnh trùng với tồn kho hiện tại, không có gì thay đổi");

                inv.Quantity = request.NewQuantity;
                inv.UpdatedAt = DateTime.UtcNow;

                var txRecord = new InventoryTransaction
                {
                    VariantId = request.VariantId,
                    Type = InventoryTransactionType.ADJUST,
                    Quantity = diff,
                    RefType = "Stocktake",
                    QuantityBefore = qBefore,
                    QuantityAfter = request.NewQuantity,
                    Note = request.Note ?? $"Điều chỉnh tồn kho: {qBefore} → {request.NewQuantity}",
                    CreatedBy = createdBy
                };
                _db.InventoryTransactions.Add(txRecord);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ─── GET ALL (PAGED) ──────────────────────────────────
        public async Task<PagedResult<InventoryResponseDto>> GetAllAsync(
            bool? lowStockOnly, Guid? productId, string? keyword,
            int page, int pageSize)
        {
            var query = _db.Inventories
                .Include(i => i.Variant)
                    .ThenInclude(v => v.Product)
                .AsQueryable();

            if (lowStockOnly == true)
                query = query.Where(i => i.Quantity <= i.MinStock);
            if (productId.HasValue)
                query = query.Where(i => i.Variant.ProductId == productId.Value);
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(i =>
                    i.Variant.Sku.Contains(keyword) ||
                    i.Variant.Product.Name.Contains(keyword));

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(i => i.Variant.Product.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => _mapper.Map<InventoryResponseDto>(i))
                .ToListAsync();

            return new PagedResult<InventoryResponseDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}