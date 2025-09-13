using ConferenceWebApp.Application.DTOs.RefferReport;
using ConferenceWebApp.Application.ViewModels;

namespace ConferenceWebApp.Infrastructure.Services.Abstract;

public interface IReportsReferralService
{
    Task<ApprovedReportsForReferralViewModel> GetApprovedReportsForReferral(Guid userId);

    Task<RefferSearchDTO> SearchUsersForReferral(Guid reportId, string query);

    Task ReferReport(Guid reportId, Guid targetUserId, Guid currentUserId);
    Task ConfirmTransfer(Guid reportId, Guid targetUserId);
    Task CancelTransfer(Guid reportId, Guid userId);
}