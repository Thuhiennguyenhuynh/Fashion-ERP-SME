using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Domain.Entities;
using FashionERP.Infrastructure.Data;

namespace FashionERP.API.Controllers
{
    public class CategoriesController : BaseController
    {
        private readonly AppDbContext _db;
        public CategoriesController(AppDbContext db) { _db = db; }

        public record CreateCategoryDto(
            string Name, string Slug, string? Description,
            Guid? ParentId, int SortOrder = 0);

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Categories
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .ToListAsync();

            var result = list.Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.SortOrder,
                c.IsActive,
                ParentId = c.ParentId,
                ParentName = c.Parent?.Name,
                ChildCount = c.Children.Count
            });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto request)
        {
            if (await _db.Categories.AnyAsync(c => c.Slug == request.Slug.Trim()))
                throw new DuplicateException($"Slug '{request.Slug}' đã được sử dụng");

            if (await _db.Categories.AnyAsync(c => c.Name == request.Name.Trim()))
                throw new DuplicateException($"Tên danh mục '{request.Name}' đã tồn tại");

            if (request.ParentId.HasValue &&
                !await _db.Categories.AnyAsync(c => c.Id == request.ParentId.Value))
                throw new NotFoundException("Danh mục cha", request.ParentId.Value);

            var cat = new Category
            {
                Name = request.Name.Trim(),
                Slug = request.Slug.Trim().ToLower(),
                Description = request.Description?.Trim(),
                ParentId = request.ParentId,
                SortOrder = request.SortOrder,
                IsActive = true
            };
            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();
            return Created(new { cat.Id, cat.Name, cat.Slug }, "Thêm danh mục thành công");
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryDto request)
        {
            var cat = await _db.Categories.FindAsync(id)
                ?? throw new NotFoundException("Danh mục", id);

            if (await _db.Categories.AnyAsync(c => c.Slug == request.Slug.Trim() && c.Id != id))
                throw new DuplicateException($"Slug '{request.Slug}' đã được sử dụng");

            if (id == request.ParentId)
                throw new BusinessException("Danh mục không thể là cha của chính nó");

            cat.Name = request.Name.Trim();
            cat.Slug = request.Slug.Trim().ToLower();
            cat.Description = request.Description?.Trim();
            cat.ParentId = request.ParentId;
            cat.SortOrder = request.SortOrder;
            cat.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok<object>(null!, "Cập nhật danh mục thành công");
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var cat = await _db.Categories.FindAsync(id)
                ?? throw new NotFoundException("Danh mục", id);

            if (await _db.Categories.AnyAsync(c => c.ParentId == id))
                throw new BusinessException("Không thể xóa danh mục đang có danh mục con");

            if (await _db.Products.AnyAsync(p => p.CategoryId == id))
                throw new BusinessException("Không thể xóa danh mục đang có sản phẩm");

            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();
            return Ok<object>(null!, "Xóa danh mục thành công");
        }
    }
}

