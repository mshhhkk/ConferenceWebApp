using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Application;

namespace ConferenceWebApp.Infrastructure.Services.Abstract;

public interface IExtendedThesisService
{
    Task<Result<ExtendedThesisDTO>> GetExtendedThesisesAsync(Guid userId);

    Task<Result<EditExtendedThesisDTO>> GetThesisAsync(Guid userId, Guid reportId);

    Task<Result> UpdateExtendedThesisAsync(Guid userId, EditExtendedThesisDTO dto);
}