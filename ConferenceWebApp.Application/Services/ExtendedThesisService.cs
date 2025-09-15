using ConferenceWebApp.Domain.Enums;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using ConferenceWebApp.Application;
using ConferenceWebApp.Application.Interfaces.Repositories;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class ExtendedThesisService : IExtendedThesisService
{
    private readonly IReportsRepository _reportsRepository;
    private readonly IUserProfileRepository _userProfileRepository;

    public ExtendedThesisService(
        IReportsRepository reportsRepository,
        IUserProfileRepository userProfileRepository)
    {
        _reportsRepository = reportsRepository;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<Result<ExtendedThesisDTO>> GetExtendedThesisesAsync(Guid userId)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile == null || profile.Status < ParticipantStatus.ProfileCompleted)
            return Result<ExtendedThesisDTO>.Failure("Вы не зарегистрированы на конференцию.");

        var reports = await _reportsRepository.GetApprovedReportsByUserIdAsync(userId);
        if (reports == null || reports.Count == 0)
            return Result<ExtendedThesisDTO>.Failure("У вас нет одобренных докладов.");

        var dto = new ExtendedThesisDTO
        {
            ReportsWithTheses = reports.Where(r => r.ExtThesis != null && r.Status == ReportStatus.ThesisApproved)
                                      .Select(r => new Reports
                                      {
                                          Id = r.Id,
                                          ReportTheme = r.ReportTheme,
                                          Section = r.Section,
                                          WorkType = r.WorkType,
                                          UploadedAt = r.UploadedAt,
                                          LastUpdatedAt = r.LastUpdatedAt,
                                      }).ToList(),
            ReportsWithoutTheses = reports.Where(r => r.ExtThesis == null && r.Status == ReportStatus.ThesisApproved)
                                         .Select(r => new Reports
                                         {
                                             Id = r.Id,
                                             ReportTheme = r.ReportTheme,
                                             Section = r.Section,
                                             WorkType = r.WorkType,
                                             UploadedAt = r.UploadedAt,
                                             LastUpdatedAt = r.LastUpdatedAt,
                                         }).ToList()
        };

        return Result<ExtendedThesisDTO>.Success(dto);
    }


    public async Task<Result<EditExtendedThesisDTO>> GetThesisAsync(Guid userId, Guid reportId)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile == null || profile.Status < ParticipantStatus.ProfileCompleted)
            return Result<EditExtendedThesisDTO>.Failure("Вы не зарегистрированы на конференцию.");

        var report = await _reportsRepository.GetReportByIdAsync(reportId);

        if (report == null)
            return Result<EditExtendedThesisDTO>.Failure("Доклад не найден");

        if (report.Status < ReportStatus.ThesisApproved)
            return Result<EditExtendedThesisDTO>.Failure("У вас нет доступных одобренных тезисов");

        var reports = await _reportsRepository.GetApprovedReportsByUserIdAsync(userId);

        var dto = new EditExtendedThesisDTO
        {
            ReportId = report.Id,
            ReportTheme = report.ReportTheme,
            Organization = profile.Organization!,
            ExtThesis = report.ExtThesis
        };
     
        return Result<EditExtendedThesisDTO>.Success(dto);
    }

    public async Task<Result> UpdateExtendedThesisAsync(Guid userId, EditExtendedThesisDTO dto)
    {
        var report = await _reportsRepository.GetReportByIdAsync(dto.ReportId);
        if (report == null)
            return Result.Failure("Доклад не найден");
        if (report.Status < ReportStatus.ThesisApproved)
            return Result.Failure("У вас нет доступных одобренных тезисов");

        report.ExtThesis = dto.ExtThesis;
        report.Status = ReportStatus.SubmittedExtendedThesis;
        await _reportsRepository.UpdateReportAsync(report);

        return Result.Success();
    }
}