using ConferenceWebApp.Application.DTOs.RefferReport;
using ConferenceWebApp.Application.DTOs.ReportsRefferDTOs;

namespace ConferenceWebApp.Application.Interfaces.Services;

public interface IReportsReferralService
{
    Task<Result<ApprovedReportsForReferralDTO>> GetApprovedReportsForReferral(Guid userId);

    Task<RefferSearchDTO> SearchUsersForReferral(Guid reportId, string query);

    Task<Result> ReferReport(Guid reportId, Guid targetUserId, Guid currentUserId);

    Task<Result> ConfirmTransfer(Guid reportId, Guid targetUserId);
    Task<Result> CancelTransfer(Guid reportId, Guid userId);
}