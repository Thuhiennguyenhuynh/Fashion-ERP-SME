using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.HR;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly AppDbContext _db;

        public LeaveService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<LeaveResponseDto> CreateAsync(CreateLeaveRequestDto request)
        {
            var leave = new Leave
            {
                EmployeeId = request.EmployeeId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Reason = request.Reason,
                Days = (int)(request.ToDate - request.FromDate).TotalDays + 1,
                Status = LeaveStatus.Pending
            };
            _db.Leaves.Add(leave);
            await _db.SaveChangesAsync();
            return new LeaveResponseDto { Id = leave.Id, Status = leave.Status.ToString() };
        }

        public async Task<LeaveResponseDto> ApproveAsync(Guid id, ApproveLeaveRequestDto request, Guid approverId)
        {
            var leave = await _db.Leaves.FindAsync(id) ?? throw new NotFoundException("Đơn nghỉ phép", id);
            leave.Status = Enum.Parse<LeaveStatus>(request.Status);
            leave.ApprovedBy = approverId;
            await _db.SaveChangesAsync();
            return new LeaveResponseDto { Id = leave.Id, Status = leave.Status.ToString() };
        }

        public async Task<List<LeaveResponseDto>> GetByEmployeeAsync(Guid employeeId)
        {
            return await _db.Leaves.Where(l => l.EmployeeId == employeeId)
                .Select(l => new LeaveResponseDto { Id = l.Id, Status = l.Status.ToString() })
                .ToListAsync();
        }

        public async Task<List<LeaveResponseDto>> GetPendingAsync()
        {
            return await _db.Leaves.Where(l => l.Status == LeaveStatus.Pending)
                .Select(l => new LeaveResponseDto { Id = l.Id, Status = l.Status.ToString() })
                .ToListAsync();
        }
    }
}