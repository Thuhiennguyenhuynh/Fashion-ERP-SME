using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionERP.Infrastructure.Data;

namespace FashionERP.API.Controllers
{
    [Authorize]
    public class VariantsController : BaseController
    {
        private readonly AppDbContext _db;
        public VariantsController(AppDbContext db) => _db = db;

        /// <summary>Tra cứu biến thể theo barcode — dùng cho POS scan</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByBarcode([FromQuery] string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return BadRequest("Barcode không được để trống");

            var variant = await _db.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.Barcode == barcode && v.IsActive)
                .Select(v => new
                {
                    v.Id,
                    v.Sku,
                    v.Barcode,
                    v.Size,
                    v.Color,
                    v.ColorHex,
                    v.Price,
                    v.ImageUrl,
                    Product = new
                    {
                        v.Product.Id,
                        v.Product.Name,
                        v.Product.MainImageUrl
                    }
                })
                .FirstOrDefaultAsync();

            if (variant == null)
                return NotFound("Không tìm thấy biến thể với mã vạch này");

            return Ok(variant);
        }
    }
}