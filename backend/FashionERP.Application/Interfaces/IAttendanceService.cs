namespace FashionERP.Application.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FashionERP.Application.DTOs.HR;

    public interface IAttendanceService
    {
        Task<AttendanceResponseDto> CheckInAsync(CheckInRequestDto request);
        Task<AttendanceResponseDto> CheckOutAsync(CheckOutRequestDto request);
        Task<AttendanceResponseDto> CreateManualAsync(CreateAttendanceManualDto request);
        Task<List<AttendanceResponseDto>> GetByEmployeeAsync(Guid employeeId, int month, int year);
    }
}
