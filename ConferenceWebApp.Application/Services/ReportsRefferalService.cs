using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs.RefferReport;
using ConferenceWebApp.Application.DTOs.ReportsRefferDTOs;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class ReportsReferralService : IReportsReferralService
{
    private readonly IReportsRepository _reportsRepository;
    private readonly IUserProfileRepository _userProfileRepository;

    public ReportsReferralService(
        IReportsRepository reportsRepository,
        IUserProfileRepository userProfileRepository)
    {
        _reportsRepository = reportsRepository;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<Result<ApprovedReportsForReferralDTO>> GetApprovedReportsForReferral(Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        var approvedReports = await _reportsRepository.GetApprovedReportsByUserIdAsync(userId);

        var reports = approvedReports
            .Select(report => new ApprovedReportToRefferalDTO
            {
                UserId = userId,
                ReportId = report.Id,
                Title = report.ReportTheme,
                ExtThesis = report.ExtThesis ?? string.Empty,
            }).ToList();

        if (reports == null)
        {
            return Result<ApprovedReportsForReferralDTO>.Failure("У вас нет одобренных расширенных тезисов");
        }

        var incomingTransfersEntities = await _reportsRepository.GetApprovedReportsAsync();
        var incomingTransfers = incomingTransfersEntities
            .Where(r => r.TargetUserId == userId && r.TransferStatus == ReportTransferStatus.Requested)
            .Select(report => new ApprovedReportToRefferalDTO
            {
                UserId = report.UserId,
                ReportId = report.Id,
                Title = report.ReportTheme,
                ExtThesis = report.ExtThesis ?? string.Empty,
            }).ToList();

        var dto = new ApprovedReportsForReferralDTO
        {
            Reports = reports,
            IncomingTransfers = incomingTransfers
        };

        return Result<ApprovedReportsForReferralDTO>.Success(dto);
    }


    public async Task<RefferSearchDTO> SearchUsersForReferral(Guid reportId, string query)
    {
        var users = string.IsNullOrWhiteSpace(query)
            ? new List<UserProfile>()
            : await _userProfileRepository.SearchByFullNameAsync(query);

        var userDtos = users.Select(user => new UserSearchResultDTO
        {
            UserId = user.UserId,
            FullName = $"{user.FirstName} {user.LastName}"
        }).ToList();

        return new RefferSearchDTO
        {
            ReportId = reportId,
            Query = query,
            Users = userDtos
        };
    }

    public async Task<Result> ReferReport(Guid reportId, Guid targetUserId, Guid currentUserId)
    {
        var report = await _reportsRepository.GetReportByIdAsync(reportId);
        if (report == null)
        {
            return Result.Failure("Доклад не найден");
        }

        var targetUser = await _userProfileRepository.GetByUserIdAsync(targetUserId);
        if (targetUser == null)
        {
            return Result.Failure("Такого пользователя не существует");
        }

        report.TransferStatus = ReportTransferStatus.Requested;
        report.LastUpdatedAt = DateTime.UtcNow;
        report.AuthorId = currentUserId;
        report.UserId = currentUserId;
        report.TargetUserId = targetUserId;

        await _reportsRepository.UpdateReportAsync(report);

        return Result.Success();
    }

    public async Task<Result> ConfirmTransfer(Guid reportId, Guid targetUserId)
    {
        var report = await _reportsRepository.GetReportByIdAsync(reportId);
        if (report == null)
        {
            return Result.Failure("Доклад не найден");
        }

        var newReport = new Reports
        {
            Id = Guid.NewGuid(),
            ReportTheme = report.ReportTheme,
            ExtThesis = report.ExtThesis,
            Section = report.Section,
            WorkType = report.WorkType,
            FilePath = report.FilePath,
            UploadedAt = DateTime.UtcNow,
            UserId = targetUserId,
            IsAuthor = false,
            AuthorId = report.AuthorId,
            TransferStatus = ReportTransferStatus.Confirmed,
            Status = ReportStatus.ExtendedThesisApproved
        };

        await _reportsRepository.AddReportAsync(newReport);
        await _reportsRepository.DeleteReportAsync(report.Id);

        return Result.Success();
    }


    public async Task<Result> CancelTransfer(Guid reportId, Guid userId)
    {
        var report = await _reportsRepository.GetReportByIdAsync(reportId);
        if (report == null)
        {
            return Result.Failure("Доклад не найден");
        }

        if (report.UserId != userId)
        {
            return Result.Failure("Нет прав на отмену передачи");
        }

        report.TransferStatus = ReportTransferStatus.Rejected;
        report.TargetUserId = Guid.Empty;
        report.LastUpdatedAt = DateTime.UtcNow;

        await _reportsRepository.UpdateReportAsync(report);

        return Result.Success();
    }


}
