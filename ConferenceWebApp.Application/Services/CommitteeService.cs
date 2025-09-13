using ConferenceWebApp.Application.DTOs.CommiteeDTOs;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using ConferenceWebApp.Application;
using ConferenceWebApp.Application.Interfaces.Repositories;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class CommitteeService : ICommitteeService
{
    private readonly ICommitteRepository _committeeRepository;

    public CommitteeService(
        ICommitteRepository committeeRepository)
    {
        _committeeRepository = committeeRepository;
    }

    public async Task<Result<List<CommitteeDTO>>> GetAllCommitteesAsync()
    {
        try
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
        catch (Exception ex)
        {
            return Result<List<CommitteeDTO>>.Failure("Произошла ошибка при получении списка комитетов");
        }
    }
}