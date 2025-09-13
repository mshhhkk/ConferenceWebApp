using ConferenceWebApp.Application;

namespace ConferenceWebApp.Infrastructure.Services.Abstract;

public interface IUserSecurityService
{
    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

    Task<Result> DeleteAccountAsync(Guid userId);
}