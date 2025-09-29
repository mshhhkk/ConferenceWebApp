using ConferenceWebApp.Application;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

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
        _logger.LogInformation("Запрос на смену пароля. UserId={UserId}", userId);

        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning("Пользователь не найден при смене пароля. UserId={UserId}", userId);
                return Result.Failure("Пользователь не найден.");
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Не удалось сменить пароль. UserId={UserId}. Ошибки: {Errors}", user.Id, errors);
                return Result.Failure(errors);
            }

            _logger.LogInformation("Пароль успешно изменён. UserId={UserId}", user.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении пароля. UserId={UserId}", userId);
            return Result.Failure("Произошла ошибка при изменении пароля.");
        }
    }

    public async Task<Result> DeleteAccountAsync(Guid userId)
    {
        _logger.LogInformation("Запрос на удаление аккаунта. UserId={UserId}", userId);

        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning("Пользователь не найден при удалении аккаунта. UserId={UserId}", userId);
                return Result.Failure("Пользователь не найден.");
            }

            var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (userProfile != null)
            {
                await _userProfileRepository.DeleteAsync(userProfile);
                _logger.LogInformation("Профиль пользователя удалён. UserId={UserId}", userId);
            }
            else
            {
                _logger.LogInformation("Профиль пользователя не найден, пропускаем удаление профиля. UserId={UserId}", userId);
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Не удалось удалить аккаунт. UserId={UserId}. Ошибки: {Errors}", userId, errors);
                return Result.Failure(errors);
            }

            _logger.LogInformation("Аккаунт пользователя успешно удалён. UserId={UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении аккаунта. UserId={UserId}", userId);
            return Result.Failure("Произошла ошибка при удалении аккаунта.");
        }
    }
}
