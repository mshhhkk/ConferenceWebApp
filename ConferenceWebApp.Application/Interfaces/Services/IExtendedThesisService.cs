using ConferenceWebApp.Application.DTOs.ReportsDTOs;

namespace ConferenceWebApp.Application.Interfaces.Services;

public interface IExtendedThesisService
{
    Task<Result<ExtendedThesisDTO>> GetExtendedThesisesAsync(Guid userId);

    Task<Result<EditExtendedThesisDTO>> GetThesisAsync(Guid userId, Guid reportId);

    Task<Result> UpdateExtendedThesisAsync(Guid userId, EditExtendedThesisDTO dto);
}