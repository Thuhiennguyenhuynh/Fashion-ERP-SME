using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Promotion;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public PromotionService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // ─── GET ALL ──────────────────────────────────────────
        public async Task<List<PromotionResponseDto>> GetAllAsync()
        {
            var list = await _db.Promotions
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return _mapper.Map<List<PromotionResponseDto>>(list);
        }

        // ─── CREATE ───────────────────────────────────────────
        public async Task<PromotionResponseDto> CreateAsync(CreatePromotionRequestDto request)
        {
            var code = request.Code.Trim().ToUpper();

            if (await _db.Promotions.AnyAsync(p => p.Code == code))
                throw new DuplicateException($"Mã khuyến mãi '{code}' đã tồn tại");

            if (!Enum.TryParse<PromotionType>(request.Type, out var type))
                throw new AppException("Loại giảm giá không hợp lệ");

            if (request.EndDate <= request.StartDate)
                throw new BusinessException("Ngày kết thúc phải sau ngày bắt đầu");

            var promo = new Promotion
            {
                Code = code,
                Name = request.Name.Trim(),
                Type = type,
                DiscountValue = request.DiscountValue,
                MaxDiscount = request.MaxDiscount,
                MinOrderValue = request.MinOrderValue,
                UsageLimit = request.UsageLimit,
                UsedCount = 0,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true
            };

            _db.Promotions.Add(promo);
            await _db.SaveChangesAsync();
            return _mapper.Map<PromotionResponseDto>(promo);
        }

        // ─── DEACTIVATE ───────────────────────────────────────
        public async Task DeactivateAsync(Guid id)
        {
            var promo = await _db.Promotions.FindAsync(id)
                ?? throw new NotFoundException("Chương trình khuyến mãi", id);

            promo.IsActive = false;
            promo.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // ─── APPLY CODE (kiểm tra + tính giảm) ───────────────
        public async Task<ApplyPromotionResponseDto> ApplyCodeAsync(ApplyPromotionRequestDto request)
        {
            var code = request.Code.Trim().ToUpper();
            var now = DateTime.UtcNow;

            var promo = await _db.Promotions.FirstOrDefaultAsync(p =>
                p.Code == code && p.IsActive &&
                p.StartDate <= now && p.EndDate >= now);

            if (promo == null)
                return new ApplyPromotionResponseDto
                {
                    IsValid = false,
                    ErrorMessage = "Mã khuyến mãi không tồn tại hoặc đã hết hạn"
                };

            if (promo.UsageLimit.HasValue && promo.UsedCount >= promo.UsageLimit.Value)
                return new ApplyPromotionResponseDto
                {
                    IsValid = false,
                    ErrorMessage = "Mã khuyến mãi đã đạt giới hạn lượt sử dụng"
                };

            if (request.OrderSubtotal < promo.MinOrderValue)
                return new ApplyPromotionResponseDto
                {
                    IsValid = false,
                    ErrorMessage = $"Đơn hàng tối thiểu {promo.MinOrderValue:N0} VNĐ mới được áp dụng mã này"
                };

            var discountAmount = promo.Type == PromotionType.Percent
                ? Math.Min(request.OrderSubtotal * promo.DiscountValue / 100,
                           promo.MaxDiscount ?? decimal.MaxValue)
                : Math.Min(promo.DiscountValue, request.OrderSubtotal);

            return new ApplyPromotionResponseDto
            {
                IsValid = true,
                DiscountAmount = Math.Round(discountAmount, 0),
                PromotionName = promo.Name
            };
        }
    }
}
