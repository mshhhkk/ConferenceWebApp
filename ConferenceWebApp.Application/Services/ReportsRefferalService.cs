using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.RefferReport;
using ConferenceWebApp.Application.DTOs.ReportsRefferDTOs;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using ConferenceWebApp.Application.Interfaces.Repositories;

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

    public async Task<ApprovedReportsForReferralViewModel> GetApprovedReportsForReferral(Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        var approvedReports = await _reportsRepository.GetApprovedReportsByUserIdAsync(userId);

        // Доклады пользователя
        var reports = approvedReports
            .Select(report => new ApprovedReportToRefferalDTO
            {
                UserId = userId,
                ReportId = report.Id,
                Title = report.ReportTheme,
                ExtThesis = report.ExtThesis ?? string.Empty,
                IsTransferRequested = report.IsTransferRequested,
                IsTransferConfirmed = report.IsTransferConfirmed
            }).ToList();

        // Входящие заявки (выбираем доклады, где текущий пользователь - получатель)
        var incomingTransfersEntities = await _reportsRepository.GetApprovedReportsAsync();
        var incomingTransfers = incomingTransfersEntities
            .Where(r => r.TargetUserId == userId && r.IsTransferRequested && !r.IsTransferConfirmed)
            .Select(report => new ApprovedReportToRefferalDTO
            {
                UserId = report.UserId,
                ReportId = report.Id,
                Title = report.ReportTheme,
                ExtThesis = report.ExtThesis ?? string.Empty,
                IsTransferRequested = report.IsTransferRequested,
                IsTransferConfirmed = report.IsTransferConfirmed
            }).ToList();

        var userProfileDto = new UserProfileDTO
        {
            FullName = $"{userProfile.FirstName} {userProfile.LastName} {userProfile.MiddleName}".Trim(),
            Email = userProfile.User?.Email ?? string.Empty,
            PhoneNumber = userProfile.PhoneNumber ?? string.Empty,
            BirthDate = userProfile.BirthDate,
            Organization = userProfile.Organization ?? string.Empty,
            Specialization = userProfile.Specialization ?? string.Empty,
            PhotoUrl = userProfile.PhotoUrl,
            ParticipantType = userProfile.ParticipantType,
            HasPaidFee = userProfile.HasPaidFee,
            IsRegisteredForConference = userProfile.IsRegisteredForConference,
            IsApprovedAnyReports = approvedReports.Any()
        };

        return new ApprovedReportsForReferralViewModel
        {
            UserProfile = userProfileDto,
            Reports = reports,
            IncomingTransfers = incomingTransfers
        };
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

    public async Task ReferReport(Guid reportId, Guid targetUserId, Guid currentUserId)
    {
        var report = await _reportsRepository.GetReportByIdAsync(reportId);
        if (report == null)
        {
            throw new ArgumentException("Доклад не найден.");
        }

        report.IsTransferRequested = true;
        report.IsTransferConfirmed = false;
        report.LastUpdatedAt = DateTime.UtcNow;
        report.AuthorId = currentUserId;
        report.UserId = currentUserId;
        report.TargetUserId = targetUserId;

        await _reportsRepository.UpdateReportAsync(report);
    }

    public async Task ConfirmTransfer(Guid reportId, Guid targetUserId)
    {
        var report = await _reportsRepository.GetReportByIdAsync(reportId);
        if (report == null)
            throw new ArgumentException("Доклад не найден.");


        var newReport = new Reports
        {
            Id = Guid.NewGuid(),
            ReportTheme = report.ReportTheme,
            ExtThesis = report.ExtThesis,
            Section = report.Section,
            WorkType = report.WorkType,
            FilePath = report.FilePath,
            IsApproved = true,
            UploadedAt = DateTime.UtcNow,
            UserId = targetUserId,
            IsAuthor = false,
            AuthorId = report.AuthorId,
            IsTransferRequested = false,
            IsTransferConfirmed = true
        };

        await _reportsRepository.AddReportAsync(newReport);
        await _reportsRepository.DeleteReportAsync(report.Id);
    }


    public async Task<List<ReceivedReportTransferRequestDTO>> GetReceivedRequests(Guid userId)
    {
        var reports = await _reportsRepository.GetPendingReportsAsync();
        var requests = reports
            .Where(r => r.TargetUserId == userId && r.IsTransferRequested)
            .Select(r => new ReceivedReportTransferRequestDTO
            {
                RequestId = r.Id,
                ReportId = r.Id,
                Title = r.ReportTheme,
                ExtThesis = r.ExtThesis ?? string.Empty,
                FromUserId = r.AuthorId,
                RequestedAt = r.LastUpdatedAt,
                IsConfirmed = r.IsTransferConfirmed
            }).ToList();

        return requests;
    }

    public async Task CancelTransfer(Guid reportId, Guid userId)
    {
        var report = await _reportsRepository.GetReportByIdAsync(reportId);
        if (report == null)
            throw new ArgumentException("Доклад не найден.");

        // Проверяем, что отменяет автор заявки
        if (report.UserId != userId)
            throw new UnauthorizedAccessException("Нет прав на отмену передачи.");

        report.IsTransferRequested = false;
        report.IsTransferConfirmed = false;
        report.TargetUserId = Guid.Empty;
        report.LastUpdatedAt = DateTime.UtcNow;

        await _reportsRepository.UpdateReportAsync(report);
    }


}
