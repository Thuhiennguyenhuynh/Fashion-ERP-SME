using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.HR;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public PayrollService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        private IQueryable<Payroll> BaseQuery() =>
            _db.Payrolls.Include(p => p.Employee);

        // ─── GENERATE PAYROLL ─────────────────────────────────
        public async Task<PayrollResponseDto> GenerateAsync(GeneratePayrollRequestDto request)
        {
            var emp = await _db.Employees.FindAsync(request.EmployeeId)
                ?? throw new NotFoundException("Nhân viên", request.EmployeeId);

            if (await _db.Payrolls.AnyAsync(p =>
                p.EmployeeId == request.EmployeeId &&
                p.Month == request.Month &&
                p.Year == request.Year))
                throw new DuplicateException(
                    $"Bảng lương tháng {request.Month}/{request.Year} của nhân viên này đã được tạo");

            var workingDaysActual = await _db.Attendances
                .Where(a => a.EmployeeId == request.EmployeeId &&
                            a.WorkDate.Month == request.Month &&
                            a.WorkDate.Year == request.Year &&
                            a.Type != AttendanceType.Absent)
                .CountAsync();

            var totalOvertimeHours = await _db.Attendances
                .Where(a => a.EmployeeId == request.EmployeeId &&
                            a.WorkDate.Month == request.Month &&
                            a.WorkDate.Year == request.Year)
                .SumAsync(a => a.OvertimeHours);

            var hourlyRate = emp.WorkingDaysPerMonth > 0
                ? emp.BaseSalary / emp.WorkingDaysPerMonth / 8
                : 0;
            var overtimePay = Math.Round(totalOvertimeHours * hourlyRate * 1.5m, 0);

            var dailyRate = emp.WorkingDaysPerMonth > 0
                ? emp.BaseSalary / emp.WorkingDaysPerMonth
                : 0;
            var earnedSalary = Math.Round(dailyRate * workingDaysActual, 0);

            var netSalary = earnedSalary + request.Allowance + overtimePay - request.Deduction;
            if (netSalary < 0) netSalary = 0;

            var payroll = new Payroll
            {
                EmployeeId = request.EmployeeId,
                Month = request.Month,
                Year = request.Year,
                WorkingDaysActual = workingDaysActual,
                BaseSalary = emp.BaseSalary,
                Allowance = request.Allowance,
                OvertimePay = overtimePay,
                Deduction = request.Deduction,
                NetSalary = netSalary,
                Status = PayrollStatus.Draft
            };

            _db.Payrolls.Add(payroll);
            await _db.SaveChangesAsync();
            await _db.Entry(payroll).Reference(p => p.Employee).LoadAsync();
            return _mapper.Map<PayrollResponseDto>(payroll);
        }

        // ─── GET BY MONTH/YEAR ────────────────────────────────
        public async Task<List<PayrollResponseDto>> GetByMonthYearAsync(int month, int year)
        {
            var list = await BaseQuery()
                .Where(p => p.Month == month && p.Year == year)
                .OrderBy(p => p.Employee.FullName)
                .ToListAsync();
            return _mapper.Map<List<PayrollResponseDto>>(list);
        }

        // ─── GET BY EMPLOYEE / MONTH / YEAR ──────────────────
        public async Task<PayrollResponseDto?> GetByEmployeeMonthAsync(Guid employeeId, int year, int month)
        {
            var payroll = await BaseQuery()
                .FirstOrDefaultAsync(p =>
                    p.EmployeeId == employeeId &&
                    p.Year == year &&
                    p.Month == month);
            return payroll == null ? null : _mapper.Map<PayrollResponseDto>(payroll);
        }

        // ─── CONFIRM ──────────────────────────────────────────
        public async Task ConfirmAsync(Guid id)
        {
            var payroll = await _db.Payrolls.FindAsync(id)
                ?? throw new NotFoundException("Bảng lương", id);

            if (payroll.Status != PayrollStatus.Draft)
                throw new BusinessException("Chỉ có thể xác nhận bảng lương đang ở trạng thái Draft");

            payroll.Status = PayrollStatus.Confirmed;
            payroll.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // ─── MARK AS PAID ─────────────────────────────────────
        public async Task MarkAsPaidAsync(Guid id)
        {
            var payroll = await _db.Payrolls.FindAsync(id)
                ?? throw new NotFoundException("Bảng lương", id);

            if (payroll.Status != PayrollStatus.Confirmed)
                throw new BusinessException("Chỉ có thể đánh dấu đã trả lương khi bảng lương đã được xác nhận");

            payroll.Status = PayrollStatus.Paid;
            payroll.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}