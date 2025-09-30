using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services.Admin;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Infrastructure.Services.Realization.Admin;

public class ReportAdminService : IReportAdminService
{
    private readonly IReportsRepository _reportsRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ReportAdminService> _logger;

    public ReportAdminService(
        IReportsRepository reportsRepository,
        IUserProfileRepository userProfileRepository,
        UserManager<User> userManager,
        ILogger<ReportAdminService> logger)
    {
        _reportsRepository = reportsRepository;
        _userProfileRepository = userProfileRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<AdminFilteredReportsListDTO> GetFilteredReportsAsync(string? search)
    {
        _logger.LogInformation("Админ-фильтр докладов. Search='{Search}'", search);

        var pendingReports = await _reportsRepository.GetPendingReportsAsync();
        var approvedReports = await _reportsRepository.GetApprovedReportsAsync();
        var rejectedReports = await _reportsRepository.GetRejectedReportsAsync();
        var pendingExtendedTheses = await _reportsRepository.GetPendingExtendedThesesAsync();
        var approvedExtendedTheses = await _reportsRepository.GetApprovedExtendedThesesAsync();
        var rejectedExtendedTheses = await _reportsRepository.GetRejectedExtendedThesesAsync();

        List<UserProfile>? filteredUsers = null;
        if (!string.IsNullOrWhiteSpace(search))
        {
            filteredUsers = await _userProfileRepository.SearchByFullNameAsync(search);
            _logger.LogInformation("Найдено пользователей по поиску: {Count}", filteredUsers.Count);
        }

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
                    {
                        dto.AuthorFullName = $"{profile.LastName} {profile.FirstName} {profile.MiddleName}";
                        dto.Organization = profile.Organization;
                    }
                }

                dtos.Add(dto);
            }
            _logger.LogInformation("Сформировано {Count} DTO", dtos.Count);
            return dtos;
        }

        var result = new AdminFilteredReportsListDTO
        {
            SearchQuery = search ?? "",
            PendingReports = await Map(Filter(pendingReports)),
            ApprovedReports = await Map(Filter(approvedReports)),
            RejectedReports = await Map(Filter(rejectedReports)),
            PendingExtendedTheses = await Map(Filter(pendingExtendedTheses)),
            ApprovedExtendedTheses = await Map(Filter(approvedExtendedTheses)),
            RejectedExtendedTheses = await Map(Filter(rejectedExtendedTheses)),
        };

        _logger.LogInformation("Фильтр готов: Pending={P}, Approved={A}, Rejected={R}, Pxt={PX}, Axt={AX}, Rxt={RX}",
            result.PendingReports.Count,
            result.ApprovedReports.Count,
            result.RejectedReports.Count,
            result.PendingExtendedTheses.Count,
            result.ApprovedExtendedTheses.Count,
            result.RejectedExtendedTheses.Count);

        return result;
    }

    public async Task<string?> GetReportFilePathAsync(Guid id)
    {
        var report = await _reportsRepository.GetReportByIdAsync(id);
        if (report == null)
        {
            _logger.LogWarning("Файл доклада: доклад не найден. ReportId={ReportId}", id);
            return null;
        }

        _logger.LogInformation("Файл доклада найден. ReportId={ReportId}, Path='{Path}'", id, report.FilePath);
        return string.IsNullOrEmpty(report.FilePath) ? null : report.FilePath;
    }

    public async Task<bool> ApproveReportAsync(Guid id)
    {
        _logger.LogInformation("Одобрение тезиса. ReportId={ReportId}", id);
        var report = await _reportsRepository.GetReportByIdAsync(id);
        var userProfile = await _userProfileRepository.GetUserProfileByReportIdAsync(id);

        if (userProfile == null || report == null)
        {
            _logger.LogWarning("Невозможно одобрить тезис: report={HasReport}, profile={HasProfile}. ReportId={ReportId}",
                report != null, userProfile != null, id);
            return false;
        }

        report.Status = ReportStatus.ThesisApproved;
        userProfile.ParticipantType = ParticipantType.Speaker;
        userProfile.ApprovalStatus = UserApprovalStatus.ThesisApproved;
        await _userProfileRepository.UpdateAsync(userProfile);
        await _reportsRepository.UpdateReportAsync(report);

        _logger.LogInformation("Тезис одобрен. ReportId={ReportId}", id);
        return true;
    }

    public async Task<bool> RollbackReportAsync(Guid id)
    {
        _logger.LogInformation("Откат одобрения тезиса. ReportId={ReportId}", id);
        var report = await _reportsRepository.GetReportByIdAsync(id);
        var userProfile = await _userProfileRepository.GetUserProfileByReportIdAsync(id);

        if (userProfile == null || report == null)
        {
            _logger.LogWarning("Невозможно откатить: report={HasReport}, profile={HasProfile}. ReportId={ReportId}",
                report != null, userProfile != null, id);
            return false;
        }

        report.Status = ReportStatus.SubmittedThesis;
        userProfile.ParticipantType = ParticipantType.Spectator;
        await _reportsRepository.UpdateReportAsync(report);

        _logger.LogInformation("Откат выполнен. ReportId={ReportId}", id);
        return true;
    }

    public async Task<Reports?> GetReportByIdAsync(Guid id)
    {
        var report = await _reportsRepository.GetReportByIdAsync(id);
        _logger.LogInformation("Получение доклада по ID: {ReportId}. Найден={Found}", id, report != null);
        return report;
    }

    public async Task RejectReportAsync(Guid id, string comment)
    {
        _logger.LogInformation("Возврат тезиса на доработку. ReportId={ReportId}", id);
        var report = await _reportsRepository.GetReportByIdAsync(id);
        if (report == null)
        {
            _logger.LogWarning("Невозможно вернуть тезис на доработку — доклад не найден. ReportId={ReportId}", id);
            return;
        }

        report.Status = ReportStatus.ThesisReturnedForCorrection;
        report.RejectionComment = comment;
        await _reportsRepository.UpdateReportAsync(report);

        _logger.LogInformation("Тезис возвращён на доработку. ReportId={ReportId}", id);
    }

    public async Task<bool> ApproveExtendedThesisAsync(Guid id)
    {
        _logger.LogInformation("Одобрение расширенных тезисов. ReportId={ReportId}", id);
        var report = await _reportsRepository.GetReportByIdAsync(id);
        if (report == null)
        {
            _logger.LogWarning("Нельзя одобрить расширенные тезисы — доклад не найден. ReportId={ReportId}", id);
            return false;
        }
        var user = await _userProfileRepository.GetUserProfileByReportIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("У доклада не найден владелец,ReportId={ReportId}", id);
            return false;
        }
        report.Status = ReportStatus.ExtendedThesisApproved;
        user.ApprovalStatus = UserApprovalStatus.ThesisApproved;
        await _reportsRepository.UpdateReportAsync(report);
        await _userProfileRepository.UpdateAsync(user);
        _logger.LogInformation("Расширенные тезисы одобрены. ReportId={ReportId}", id);
        return true;
    }

    public async Task<bool> RejectExtendedThesisAsync(Guid id, string comment)
    {
        _logger.LogInformation("Отклонение расширенных тезисов. ReportId={ReportId}", id);
        var report = await _reportsRepository.GetReportByIdAsync(id);
        if (report == null)
        {
            _logger.LogWarning("Нельзя отклонить расширенные тезисы — доклад не найден. ReportId={ReportId}", id);
            return false;
        }

        report.Status = ReportStatus.ExtendedThesisReturnedForCorrection;
        report.RejectionComment = comment;
        await _reportsRepository.UpdateReportAsync(report);

        _logger.LogInformation("Расширенные тезисы отклонены. ReportId={ReportId}", id);
        return true;
    }
}
