using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application;

namespace ConferenceWebApp.Infrastructure.Services.Abstract.Admin;

public interface IAdminPaymentService
{
    Task<Result<List<UserWithReceiptDTO>>> GetUsersWithReceiptsAsync();

    Task<Result> MarkPaymentAsPaidAsync(Guid userId);
}