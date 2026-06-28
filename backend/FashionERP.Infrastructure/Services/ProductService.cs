using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Product;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Common;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public ProductService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        private static readonly Dictionary<string, Expression<Func<Product, object>>> SortMap = new()
        {
            ["name"] = x => x.Name,
            ["price"] = x => x.BasePrice,
            ["createdat"] = x => x.CreatedAt
        };

        private IQueryable<Product> BaseQuery() =>
            _db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants).ThenInclude(v => v.Inventory);

        // ─── GET ALL (paging + filter + smart search + sort) ──
        public async Task<PagedResult<ProductResponseDto>> GetAllAsync(ProductQueryParams p)
        {
            var query = BaseQuery();

            ProductStatus statusFilter = default;
            var hasStatusFilter = !string.IsNullOrEmpty(p.Status) && Enum.TryParse(p.Status, out statusFilter);

            query = query
                .WhereIf(hasStatusFilter, x => x.Status == statusFilter)
                .WhereIf(p.CategoryId.HasValue, x => x.CategoryId == p.CategoryId)
                .WhereIf(p.BrandId.HasValue, x => x.BrandId == p.BrandId)
                .WhereIf(!string.IsNullOrEmpty(p.Gender), x => x.Gender.ToString() == p.Gender)
                .WhereIf(p.MinPrice.HasValue, x => x.BasePrice >= p.MinPrice)
                .WhereIf(p.MaxPrice.HasValue, x => x.BasePrice <= p.MaxPrice)
                .WhereIf(!string.IsNullOrEmpty(p.Size),
                    x => x.Variants.Any(v => v.Size.ToString() == p.Size && v.IsActive))
                .WhereIf(!string.IsNullOrEmpty(p.Color),
                    x => x.Variants.Any(v => v.Color.ToLower().Contains(p.Color!.Trim().ToLower()) && v.IsActive))
                .WhereIf(p.InStock == true,
                    x => x.Variants.Any(v => v.Inventory != null && v.Inventory.Quantity > 0))
                .WhereIf(p.InStock == false,
                    x => x.Variants.All(v => v.Inventory == null || v.Inventory.Quantity == 0));

            query = query.SmartSearch(p.Keyword, x => x.Name, x => x.ProductCode, x => x.Tags);
            query = query.ApplySort(p.SortBy, SortMap);

            var paged = await query.ToPagedResultAsync(p.Page, p.PageSize);
            return paged.MapTo(items => _mapper.Map<List<ProductResponseDto>>(items));
        }

        // ─── GET BY ID ────────────────────────────────────────
        public async Task<ProductResponseDto> GetByIdAsync(Guid id)
        {
            var product = await BaseQuery().FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException("Sản phẩm", id);
            return _mapper.Map<ProductResponseDto>(product);
        }

        // ─── CREATE ───────────────────────────────────────────
        public async Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId))
                throw new NotFoundException("Danh mục", request.CategoryId);

            if (request.BrandId.HasValue && !await _db.Brands.AnyAsync(b => b.Id == request.BrandId.Value))
                throw new NotFoundException("Thương hiệu", request.BrandId.Value);

            if (!Enum.TryParse<ProductGender>(request.Gender, out var gender))
                throw new AppException("Đối tượng sử dụng không hợp lệ");

            if (!Enum.TryParse<ProductStatus>(request.Status, out var status))
                status = ProductStatus.Draft;

            var year = DateTime.UtcNow.Year;
            var countThisYear = await _db.Products
                .CountAsync(p => p.ProductCode.StartsWith($"PROD-{year}-"));
            var productCode = $"PROD-{year}-{(countThisYear + 1):D4}";

            var product = new Product
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                Gender = gender,
                BasePrice = request.BasePrice,
                Tags = request.Tags?.Trim(),
                Status = status,
                ProductCode = productCode
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(product.Id);
        }

        // ─── UPDATE ───────────────────────────────────────────
        public async Task<ProductResponseDto> UpdateAsync(Guid id, UpdateProductRequestDto request)
        {
            var product = await _db.Products.FindAsync(id)
                ?? throw new NotFoundException("Sản phẩm", id);

            if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId))
                throw new NotFoundException("Danh mục", request.CategoryId);

            if (request.BrandId.HasValue && !await _db.Brands.AnyAsync(b => b.Id == request.BrandId.Value))
                throw new NotFoundException("Thương hiệu", request.BrandId.Value);

            if (!Enum.TryParse<ProductGender>(request.Gender, out var gender))
                throw new AppException("Đối tượng sử dụng không hợp lệ");

            if (!Enum.TryParse<ProductStatus>(request.Status, out var status))
                throw new AppException("Trạng thái sản phẩm không hợp lệ");

            product.Name = request.Name.Trim();
            product.Description = request.Description?.Trim();
            product.CategoryId = request.CategoryId;
            product.BrandId = request.BrandId;
            product.Gender = gender;
            product.BasePrice = request.BasePrice;
            product.Tags = request.Tags?.Trim();
            product.Status = status;
            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        // ─── DELETE (soft delete) ──────────────────────────────
        public async Task DeleteAsync(Guid id)
        {
            var product = await _db.Products.FindAsync(id)
                ?? throw new NotFoundException("Sản phẩm", id);

            var hasOrders = await _db.OrderItems.AnyAsync(oi => oi.Variant.ProductId == id);
            if (hasOrders)
                throw new BusinessException("Sản phẩm đã có trong đơn hàng, không thể xóa cứng. Đã chuyển sang Archived.");

            product.MarkDeleted();
            product.Status = ProductStatus.Archived;
            product.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // ─── UPDATE MAIN IMAGE ────────────────────────────────
        public async Task UpdateMainImageAsync(Guid id, string imageUrl, string publicId)
        {
            var product = await _db.Products.FindAsync(id)
                ?? throw new NotFoundException("Sản phẩm", id);

            product.MainImageUrl = imageUrl;
            product.MainImagePublicId = publicId;
            product.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // ─── ADD VARIANT ──────────────────────────────────────
        public async Task<ProductVariantResponseDto> AddVariantAsync(CreateVariantRequestDto request)
        {
            if (!await _db.Products.AnyAsync(p => p.Id == request.ProductId))
                throw new NotFoundException("Sản phẩm", request.ProductId);

            if (!Enum.TryParse<ProductSize>(request.Size, out var size))
                throw new AppException("Kích cỡ không hợp lệ");

            var dup = await _db.ProductVariants.AnyAsync(v =>
                v.ProductId == request.ProductId &&
                v.Size == size &&
                v.Color.ToLower() == request.Color.Trim().ToLower());
            if (dup)
                throw new DuplicateException($"Biến thể {request.Size} - {request.Color} đã tồn tại cho sản phẩm này");

            var product = await _db.Products.FindAsync(request.ProductId)!;
            var colorKey = request.Color.Trim().ToUpper().Replace(" ", "");
            var sku = $"{product!.ProductCode}-{request.Size}-{colorKey}";

            var skuBase = sku;
            var suffix = 1;
            while (await _db.ProductVariants.AnyAsync(v => v.Sku == sku))
                sku = $"{skuBase}-{suffix++}";

            var variant = new ProductVariant
            {
                ProductId = request.ProductId,
                Size = size,
                Color = request.Color.Trim(),
                ColorHex = request.ColorHex?.Trim(),
                Sku = sku,
                Barcode = request.Barcode?.Trim(),
                Price = request.Price,
                IsActive = true
            };

            _db.ProductVariants.Add(variant);

            var inventory = new Inventory
            {
                VariantId = variant.Id,
                Quantity = 0,
                MinStock = 5
            };
            _db.Inventories.Add(inventory);

            await _db.SaveChangesAsync();

            await _db.Entry(variant).Reference(v => v.Inventory).LoadAsync();
            return _mapper.Map<ProductVariantResponseDto>(variant);
        }

        // ─── UPDATE VARIANT ───────────────────────────────────
        public async Task<ProductVariantResponseDto> UpdateVariantAsync(
            Guid variantId, CreateVariantRequestDto request)
        {
            var variant = await _db.ProductVariants
                .Include(v => v.Inventory)
                .FirstOrDefaultAsync(v => v.Id == variantId)
                ?? throw new NotFoundException("Biến thể sản phẩm", variantId);

            if (!Enum.TryParse<ProductSize>(request.Size, out var size))
                throw new AppException("Kích cỡ không hợp lệ");

            var dup = await _db.ProductVariants.AnyAsync(v =>
                v.ProductId == variant.ProductId &&
                v.Size == size &&
                v.Color.ToLower() == request.Color.Trim().ToLower() &&
                v.Id != variantId);
            if (dup)
                throw new DuplicateException($"Biến thể {request.Size} - {request.Color} đã tồn tại cho sản phẩm này");

            if (!string.IsNullOrEmpty(request.Barcode) &&
                await _db.ProductVariants.AnyAsync(v =>
                    v.Barcode == request.Barcode && v.Id != variantId))
                throw new DuplicateException($"Barcode '{request.Barcode}' đã được sử dụng bởi biến thể khác");

            variant.Size = size;
            variant.Color = request.Color.Trim();
            variant.ColorHex = request.ColorHex?.Trim();
            variant.Price = request.Price;
            variant.Barcode = request.Barcode?.Trim();
            variant.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return _mapper.Map<ProductVariantResponseDto>(variant);
        }

        // ─── DELETE VARIANT ───────────────────────────────────
        public async Task DeleteVariantAsync(Guid variantId)
        {
            var variant = await _db.ProductVariants.FindAsync(variantId)
                ?? throw new NotFoundException("Biến thể sản phẩm", variantId);

            if (await _db.OrderItems.AnyAsync(oi => oi.VariantId == variantId))
                throw new BusinessException("Không thể xóa biến thể đã có trong đơn hàng");

            _db.ProductVariants.Remove(variant);
            await _db.SaveChangesAsync();
        }
    }
}