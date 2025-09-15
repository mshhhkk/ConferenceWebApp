using ConferenceWebApp.Application.DTOs.Admin;

namespace ConferenceWebApp.Application.Interfaces.Services.Admin;

public interface IAdminPaymentService
{
    Task<Result<List<UserWithReceiptDTO>>> GetUsersWithReceiptsAsync();

    Task<Result> MarkPaymentAsPaidAsync(Guid userId);
}