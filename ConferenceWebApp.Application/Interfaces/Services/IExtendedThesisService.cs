using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Application;

namespace ConferenceWebApp.Infrastructure.Services.Abstract;

public interface IExtendedThesisService
{
    Task<Result<ExtendedThesisViewModel>> GetExtendedThesisesAsync(User user);

    Task<Result<EditExtendedThesisViewModel>> GetThesisAsync(User user, Guid reportId);

    Task<Result> UpdateExtendedThesisAsync(User user, EditExtendedThesisDTO dto);
}