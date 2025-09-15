using ConferenceWebApp.Application.DTOs.CommiteeDTOs;

namespace ConferenceWebApp.Application.Interfaces.Services;

public interface ICommitteeService
{
    Task<Result<List<CommitteeDTO>>> GetAllAsync();
}