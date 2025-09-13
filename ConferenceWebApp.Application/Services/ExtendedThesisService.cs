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

    public async Task<Result<ExtendedThesisViewModel>> GetExtendedThesisesAsync(User user)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);
        if (profile == null || !profile.IsRegisteredForConference)
            return Result<ExtendedThesisViewModel>.Failure("Вы не зарегистрированы на конференцию.");

        var reports = await _reportsRepository.GetApprovedReportsByUserIdAsync(user.Id);
        if (reports == null || reports.Count == 0)
            return Result<ExtendedThesisViewModel>.Failure("У вас нет одобренных докладов.");

        var userProfileDto = new UserProfileDTO
        {
            FullName = $"{profile.LastName} {profile.FirstName} {profile.MiddleName}".Trim(),
            Email = profile.User?.Email ?? string.Empty,
            PhoneNumber = profile.PhoneNumber ?? string.Empty,
            BirthDate = profile.BirthDate,
            Organization = profile.Organization ?? string.Empty,
            Specialization = profile.Specialization ?? string.Empty,
            PhotoUrl = profile.PhotoUrl,
            ParticipantType = profile.ParticipantType,
            HasPaidFee = profile.HasPaidFee,
            IsRegisteredForConference = profile.IsRegisteredForConference,
            IsApprovedAnyReports = reports.Any(r => r.IsApproved)
        };

        var vm = new ExtendedThesisViewModel
        {
            UserProfile = userProfileDto,
            ReportsWithTheses = reports.Where(r => r.ExtThesis != null)
                                      .Select(r => new Reports
                                      {
                                          Id = r.Id,
                                          ReportTheme = r.ReportTheme,
                                          Section = r.Section,
                                          WorkType = r.WorkType,
                                          UploadedAt = r.UploadedAt,
                                          LastUpdatedAt = r.LastUpdatedAt,
                                          IsApproved = r.IsApproved
                                      }).ToList(),
            ReportsWithoutTheses = reports.Where(r => r.ExtThesis == null && r.IsApproved)
                                         .Select(r => new Reports
                                         {
                                             Id = r.Id,
                                             ReportTheme = r.ReportTheme,
                                             Section = r.Section,
                                             WorkType = r.WorkType,
                                             UploadedAt = r.UploadedAt,
                                             LastUpdatedAt = r.LastUpdatedAt,
                                             IsApproved = r.IsApproved
                                         }).ToList()
        };

        return Result<ExtendedThesisViewModel>.Success(vm);
    }


    public async Task<Result<EditExtendedThesisViewModel>> GetThesisAsync(User user, Guid reportId)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);
        if (profile == null || !profile.IsRegisteredForConference)
            return Result<EditExtendedThesisViewModel>.Failure("Вы не зарегистрированы на конференцию.");

        var report = await _reportsRepository.GetReportByIdAsync(reportId);
        if (report == null || !report.IsApproved)
            return Result<EditExtendedThesisViewModel>.Failure("Доклад не найден или не одобрен.");

        var reports = await _reportsRepository.GetApprovedReportsByUserIdAsync(user.Id);


        var userProfileDto = new UserProfileDTO
        {
            FullName = $"{profile.LastName} {profile.FirstName} {profile.MiddleName}".Trim(),
            Email = profile.User?.Email ?? string.Empty,
            PhoneNumber = profile.PhoneNumber ?? string.Empty,
            BirthDate = profile.BirthDate,
            Organization = profile.Organization ?? string.Empty,
            Specialization = profile.Specialization ?? string.Empty,
            PhotoUrl = profile.PhotoUrl,
            ParticipantType = profile.ParticipantType,
            HasPaidFee = profile.HasPaidFee,
            IsRegisteredForConference = profile.IsRegisteredForConference,
            IsApprovedAnyReports = reports.Any(r => r.IsApproved)
        };

        var dto = new EditExtendedThesisDTO
        {
            ReportId = report.Id,
            ReportTheme = report.ReportTheme,
            Organization = profile.Organization!,
            ExtThesis = report.ExtThesis
        };

        var vm = new EditExtendedThesisViewModel
        {
            UserProfile = userProfileDto,
            Thesis = dto
        };
        return Result<EditExtendedThesisViewModel>.Success(vm);
    }

    public async Task<Result> UpdateExtendedThesisAsync(User user, EditExtendedThesisDTO dto)
    {
        var report = await _reportsRepository.GetReportByIdAsync(dto.ReportId);
        if (report == null || !report.IsApproved)
            return Result.Failureure("Доклад не найден или не одобрен.");

        report.ExtThesis = dto.ExtThesis;
        await _reportsRepository.UpdateReportAsync(report);

        return Result.Success();
    }
}