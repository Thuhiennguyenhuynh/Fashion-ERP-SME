namespace FashionERP.Application.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FashionERP.Application.DTOs.HR;

    public interface ILeaveService
    {
        Task<LeaveResponseDto> CreateAsync(CreateLeaveRequestDto request);
        Task<LeaveResponseDto> ApproveAsync(Guid id, ApproveLeaveRequestDto request, Guid approverId);
        Task<List<LeaveResponseDto>> GetByEmployeeAsync(Guid employeeId);
        Task<List<LeaveResponseDto>> GetPendingAsync();
    }
}
