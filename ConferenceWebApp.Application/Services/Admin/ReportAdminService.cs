using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Infrastructure.Services.Abstract.Admin;
using Microsoft.AspNetCore.Identity;
using ConferenceWebApp.Application.Interfaces.Repositories;

namespace ConferenceWebApp.Infrastructure.Services.Realization.Admin;

public class ReportAdminService : IReportAdminService
{
    private readonly IReportsRepository _reportsRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly UserManager<User> _userManager;

    public ReportAdminService(
        IReportsRepository reportsRepository,
        IUserProfileRepository userProfileRepository,
        UserManager<User> userManager)
    {
        _reportsRepository = reportsRepository;
        _userProfileRepository = userProfileRepository;
        _userManager = userManager;
    }

    public async Task<AdminFilteredReportsListDTO> GetFilteredReportsAsync(string? search)
    {
        var pending = await _reportsRepository.GetPendingReportsAsync();
        var approved = await _reportsRepository.GetApprovedReportsAsync();

        List<UserProfile>? filteredUsers = null;
        if (!string.IsNullOrWhiteSpace(search))
            filteredUsers = await _userProfileRepository.SearchByFullNameAsync(search);

        List<Reports> Filter(List<Reports> reports)
        {
            if (filteredUsers == null) return reports;
            var matchedIds = filteredUsers.Select(u => u.UserId).ToHashSet();
            return reports.Where(r => matchedIds.Contains(r.UserId)).ToList();
        }

        async Task<List<AdminReportDTO>> Map(List<Reports> reports)
        {
            var dtos = new List<AdminReportDTO>();
            foreach (var r in reports)
            {
                var dto = new AdminReportDTO
                {
                    ReportId = r.Id,
                    Title = r.ReportTheme,
                    FilePath = r.FilePath,
                    Status = r.Status,
                };

                if (r.UserId != Guid.Empty)
                {
                    var user = await _userManager.FindByIdAsync(r.UserId.ToString());
                    dto.AuthorEmail = user?.Email ?? "";

                    var profile = await _userProfileRepository.GetByUserIdAsync(r.UserId);
                    if (profile != null)
                        dto.AuthorFullName = $"{profile.LastName} {profile.FirstName} {profile.MiddleName}";
                }

                dtos.Add(dto);
            }
            return dtos;
        }

        return new AdminFilteredReportsListDTO
        {
            SearchQuery = search ?? "",
            PendingReports = await Map(Filter(pending)),
            ApprovedReports = await Map(Filter(approved))
        };
    }

    // 🚩 Теперь просто отдаём путь
    public async Task<string?> GetReportFilePathAsync(Guid id)
    {
        var report = await _reportsRepository.GetReportByIdAsync(id);
        return string.IsNullOrEmpty(report?.FilePath) ? null : report.FilePath;
    }

    public async Task<bool> ApproveReportAsync(Guid id)
    {
        var report = await _reportsRepository.GetReportByIdAsync(id);
        var userProfile = await _userProfileRepository.GetUserProfileByReportIdAsync(id);
        if (userProfile == null || report == null) return false;

        report.Status = ReportStatus.ThesisApproved;
        userProfile.ParticipantType = ParticipantType.Speaker;
        await _reportsRepository.UpdateReportAsync(report);
        return true;
    }

    public async Task<bool> RollbackReportAsync(Guid id)
    {
        var report = await _reportsRepository.GetReportByIdAsync(id);
        var userProfile = await _userProfileRepository.GetUserProfileByReportIdAsync(id);
        if (userProfile == null || report == null) return false;

        report.Status = ReportStatus.SubmittedThesis;
        userProfile.ParticipantType = ParticipantType.Spectator;
        await _reportsRepository.UpdateReportAsync(report);
        return true;
    }

    public async Task<Reports?> GetReportByIdAsync(Guid id)
    {
        return await _reportsRepository.GetReportByIdAsync(id);
    }

}
