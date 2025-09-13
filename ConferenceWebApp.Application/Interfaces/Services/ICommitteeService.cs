using ConferenceWebApp.Application.DTOs.CommiteeDTOs;
using ConferenceWebApp.Application;

namespace ConferenceWebApp.Infrastructure.Services.Abstract;

public interface ICommitteeService
{
    Task<Result<List<CommitteeDTO>>> GetAllCommitteesAsync();
}