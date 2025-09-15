using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs.CommiteeDTOs;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class CommitteeService : ICommitteeService
{
    private readonly ICommitteRepository _committeeRepository;

    public CommitteeService(
        ICommitteRepository committeeRepository)
    {
        _committeeRepository = committeeRepository;
    }

    public async Task<Result<List<CommitteeDTO>>> GetAllAsync()
    {
        var committees = await _committeeRepository.GetAllAsync();
        var committeeDtos = committees.Select(c => new CommitteeDTO
        {
            FullName = c.FullName,
            Description = c.Description,
            PhotoUrl = c.PhotoUrl,
            IsHead = c.IsHead
        }).ToList();

        return Result<List<CommitteeDTO>>.Success(committeeDtos);
    }
}
