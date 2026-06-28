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
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _db;

        public AttendanceService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<AttendanceResponseDto> CheckInAsync(CheckInRequestDto request)
        {
            var attendance = new Attendance
            {
                EmployeeId = request.EmployeeId,
                WorkDate = DateTime.Today,
                CheckIn = DateTime.UtcNow,
                Type = AttendanceType.Normal,
                Note = request.Note
            };
            _db.Attendances.Add(attendance);
            await _db.SaveChangesAsync();
            return new AttendanceResponseDto { Id = attendance.Id, WorkDate = attendance.WorkDate, CheckIn = attendance.CheckIn };
        }

        public async Task<AttendanceResponseDto> CheckOutAsync(CheckOutRequestDto request)
        {
            var attendance = await _db.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == request.EmployeeId && a.WorkDate.Date == request.WorkDate.Date)
                ?? throw new NotFoundException("Chấm công", request.EmployeeId);

            attendance.CheckOut = DateTime.UtcNow;

            if (attendance.CheckOut.HasValue && attendance.CheckIn.HasValue)
            {
                attendance.TotalHours = (decimal)(attendance.CheckOut.Value - attendance.CheckIn.Value).TotalHours;
            }

            await _db.SaveChangesAsync();
            return new AttendanceResponseDto { Id = attendance.Id, TotalHours = attendance.TotalHours };
        }
        public async Task<AttendanceResponseDto> CreateManualAsync(CreateAttendanceManualDto request)
        {
            var attendance = new Attendance
            {
                EmployeeId = request.EmployeeId,
                WorkDate = request.WorkDate,
                CheckIn = request.CheckIn,
                CheckOut = request.CheckOut,
                Type = Enum.Parse<AttendanceType>(request.Type),
                Note = request.Note
            };
            _db.Attendances.Add(attendance);
            await _db.SaveChangesAsync();
            return new AttendanceResponseDto { Id = attendance.Id };
        }

        public async Task<List<AttendanceResponseDto>> GetByEmployeeAsync(Guid employeeId, int month, int year)
        {
            return await _db.Attendances
                .Where(a => a.EmployeeId == employeeId && a.WorkDate.Month == month && a.WorkDate.Year == year)
                .Select(a => new AttendanceResponseDto { Id = a.Id, WorkDate = a.WorkDate, TotalHours = a.TotalHours })
                .ToListAsync();
        }
    }
}