using ConferenceWebApp.Application.DTOs.Admin;
using Microsoft.AspNetCore.Http;

namespace ConferenceWebApp.Application.Interfaces.Services.Admin;

public interface IAdminCommitteeService
{
    Task<List<AdminCommitteeDTO>> GetAllCommitteesAsync();

    Task<AdminCommitteeDTO?> GetCommitteeByIdAsync(Guid id);

    Task AddCommitteeAsync(AdminCommitteeDTO dto, IFormFile? photo);

    Task<bool> EditCommitteeAsync(AdminCommitteeDTO dto, IFormFile? photo);

    Task DeleteCommitteeAsync(Guid id);
}