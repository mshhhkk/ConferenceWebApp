using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using ConferenceWebApp.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ConferenceWebApp.Application.Interfaces.Repositories;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class UserSecurityService : IUserSecurityService
{
    private readonly UserManager<User> _userManager;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ILogger<UserSecurityService> _logger;

    public UserSecurityService(
        UserManager<User> userManager,
        IUserProfileRepository userProfileRepository,
        ILogger<UserSecurityService> logger)
    {
        _userManager = userManager;
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Result.Failure("Пользователь не найден.");
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            return result.Succeeded
                ? Result.Success()
                : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении пароля для пользователя {UserId}", userId);
            return Result.Failure("Произошла ошибка при изменении пароля.");
        }
    }

    public async Task<Result> DeleteAccountAsync(Guid userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Result.Failure("Пользователь не найден.");
            }

            var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (userProfile != null)
            {
                await _userProfileRepository.DeleteAsync(userProfile);
            }

            var result = await _userManager.DeleteAsync(user);

            return result.Succeeded
                ? Result.Success()
                : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении аккаунта пользователя {UserId}", userId);
            return Result.Failure("Произошла ошибка при удалении аккаунта.");
        }
    }
}