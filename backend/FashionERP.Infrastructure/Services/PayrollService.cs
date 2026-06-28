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

        public async Task<PayrollResponseDto> GenerateAsync(GeneratePayrollRequestDto request)
        {
            var emp = await _db.Employees.FindAsync(request.EmployeeId)
                ?? throw new NotFoundException("Nhân viên", request.EmployeeId);

            if (await _db.Payrolls.AnyAsync(p => p.EmployeeId == request.EmployeeId && p.Month == request.Month && p.Year == request.Year))
                throw new BusinessException($"Bảng lương tháng {request.Month}/{request.Year} của nhân viên này đã tồn tại");

            // Lấy dữ liệu chấm công trong tháng
            var attendances = await _db.Attendances
                .Where(a => a.EmployeeId == request.EmployeeId && a.WorkDate.Month == request.Month && a.WorkDate.Year == request.Year)
                .ToListAsync();

            // Tính toán công thực tế và làm thêm
            decimal workingDaysActual = attendances.Count(a => a.Type == AttendanceType.Normal || a.Type == AttendanceType.Late);
            decimal totalOvertimeHours = attendances.Sum(a => a.OvertimeHours);

            // Công thức tính lương cơ bản
            decimal hourlyRate = emp.BaseSalary / Math.Max(emp.WorkingDaysPerMonth, 1) / 8m;
            decimal overtimePay = totalOvertimeHours * hourlyRate * 1.5m; // Làm thêm hệ số 1.5

            decimal netSalary = (emp.BaseSalary / Math.Max(emp.WorkingDaysPerMonth, 1) * workingDaysActual)
                              + request.Allowance + overtimePay - request.Deduction;

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

            // Load Employee navigation property cho AutoMapper
            await _db.Entry(payroll).Reference(p => p.Employee).LoadAsync();

            return _mapper.Map<PayrollResponseDto>(payroll);
        }

        public async Task<List<PayrollResponseDto>> GetByMonthYearAsync(int month, int year)
        {
            var list = await _db.Payrolls
                .Include(p => p.Employee)
                .Where(p => p.Month == month && p.Year == year)
                .OrderBy(p => p.Employee.FullName)
                .ToListAsync();

            return _mapper.Map<List<PayrollResponseDto>>(list);
        }

        public async Task<PayrollResponseDto?> GetByEmployeeMonthAsync(Guid employeeId, int year, int month)
        {
            var payroll = await _db.Payrolls
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.Month == month && p.Year == year);

            return payroll != null ? _mapper.Map<PayrollResponseDto>(payroll) : null;
        }

        public async Task ConfirmAsync(Guid id)
        {
            var payroll = await _db.Payrolls.FindAsync(id)
                ?? throw new NotFoundException("Bảng lương", id);

            if (payroll.Status != PayrollStatus.Draft)
                throw new BusinessException("Chỉ có thể xác nhận bảng lương ở trạng thái Nháp (Draft)");

            payroll.Status = PayrollStatus.Confirmed;
            payroll.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task MarkAsPaidAsync(Guid id)
        {
            var payroll = await _db.Payrolls.FindAsync(id)
                ?? throw new NotFoundException("Bảng lương", id);

            if (payroll.Status != PayrollStatus.Confirmed)
                throw new BusinessException("Chỉ có thể thanh toán bảng lương đã được xác nhận (Confirmed)");

            payroll.Status = PayrollStatus.Paid;
            payroll.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}