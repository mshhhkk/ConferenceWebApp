namespace ConferenceWebApp.Application.Interfaces.Services;

public interface IUserSecurityService
{
    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

    Task<Result> DeleteAccountAsync(Guid userId);
}