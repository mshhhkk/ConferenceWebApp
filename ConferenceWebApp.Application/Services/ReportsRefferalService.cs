using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs.RefferReport;
using ConferenceWebApp.Application.DTOs.ReportsRefferDTOs;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class ReportsReferralService : IReportsReferralService
{
    private readonly IReportsRepository _reportsRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ILogger<ReportsReferralService> _logger;

    public ReportsReferralService(
        IReportsRepository reportsRepository,
        IUserProfileRepository userProfileRepository,
        ILogger<ReportsReferralService> logger)
    {
        _reportsRepository = reportsRepository;
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    public async Task<Result<ApprovedReportsForReferralDTO>> GetApprovedReportsForReferral(Guid userId)
    {
        _logger.LogInformation("Получение списка одобренных докладов для передачи UserId={UserId}", userId);

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

        if (reports == null || reports.Count == 0)
        {
            _logger.LogWarning("У пользователя нет одобренных расширенных тезисов UserId={UserId}", userId);
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

        _logger.LogInformation("Найдено {CountReports} докладов и {CountTransfers} входящих переводов для UserId={UserId}",
            reports.Count, incomingTransfers.Count, userId);

        var dto = new ApprovedReportsForReferralDTO
        {
            Reports = reports,
            IncomingTransfers = incomingTransfers
        };

        return Result<ApprovedReportsForReferralDTO>.Success(dto);
    }

    public async Task<RefferSearchDTO> SearchUsersForReferral(Guid reportId, string query)
    {
        _logger.LogInformation("Поиск пользователей для передачи ReportId={ReportId}, Query={Query}", reportId, query);

        var users = string.IsNullOrWhiteSpace(query)
            ? new List<UserProfile>()
            : await _userProfileRepository.SearchByFullNameAsync(query);

        var userDtos = users.Select(user => new UserSearchResultDTO
        {
            UserId = user.UserId,
            FullName = $"{user.FirstName} {user.LastName}"
        }).ToList();

        _logger.LogInformation("Найдено {Count} пользователей для передачи ReportId={ReportId}", userDtos.Count, reportId);

        return new RefferSearchDTO
        {
            ReportId = reportId,
            Query = query,
            Users = userDtos
        };
    }

    public async Task<Result> ReferReport(Guid reportId, Guid targetUserId, Guid currentUserId)
    {
        _logger.LogInformation("Попытка передачи доклада ReportId={ReportId} от UserId={CurrentUserId} к UserId={TargetUserId}",
            reportId, currentUserId, targetUserId);

        var report = await _reportsRepository.GetReportByIdAsync(reportId);
        if (report == null)
        {
            _logger.LogWarning("Доклад не найден ReportId={ReportId}", reportId);
            return Result.Failure("Доклад не найден");
        }

        var targetUser = await _userProfileRepository.GetByUserIdAsync(targetUserId);
        if (targetUser == null)
        {
            _logger.LogWarning("Пользователь для передачи не найден TargetUserId={TargetUserId}", targetUserId);
            return Result.Failure("Такого пользователя не существует");
        }

        report.TransferStatus = ReportTransferStatus.Requested;
        report.LastUpdatedAt = DateTime.UtcNow;
        report.AuthorId = currentUserId;
        report.UserId = currentUserId;
        report.TargetUserId = targetUserId;

        await _reportsRepository.UpdateReportAsync(report);

        _logger.LogInformation("Доклад успешно передан ReportId={ReportId}, TargetUserId={TargetUserId}", reportId, targetUserId);
        return Result.Success();
    }

    public async Task<Result> ConfirmTransfer(Guid reportId, Guid targetUserId)
    {
        _logger.LogInformation("Подтверждение передачи доклада ReportId={ReportId} для UserId={TargetUserId}", reportId, targetUserId);

        var report = await _reportsRepository.GetReportByIdAsync(reportId);
        if (report == null)
        {
            _logger.LogWarning("Доклад не найден при подтверждении передачи ReportId={ReportId}", reportId);
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

        _logger.LogInformation("Передача доклада подтверждена ReportId={ReportId}, новый ReportId={NewReportId}", reportId, newReport.Id);
        return Result.Success();
    }

    public async Task<Result> CancelTransfer(Guid reportId, Guid userId)
    {
        _logger.LogInformation("Попытка отмены передачи доклада ReportId={ReportId} пользователем UserId={UserId}", reportId, userId);

        var report = await _reportsRepository.GetReportByIdAsync(reportId);
        if (report == null)
        {
            _logger.LogWarning("Доклад не найден ReportId={ReportId}", reportId);
            return Result.Failure("Доклад не найден");
        }

        if (report.UserId != userId)
        {
            _logger.LogWarning("Пользователь UserId={UserId} пытался отменить передачу чужого доклада ReportId={ReportId}", userId, reportId);
            return Result.Failure("Нет прав на отмену передачи");
        }

        report.TransferStatus = ReportTransferStatus.Rejected;
        report.TargetUserId = Guid.Empty;
        report.LastUpdatedAt = DateTime.UtcNow;

        await _reportsRepository.UpdateReportAsync(report);

        _logger.LogInformation("Передача доклада отменена ReportId={ReportId} пользователем UserId={UserId}", reportId, userId);
        return Result.Success();
    }
}
