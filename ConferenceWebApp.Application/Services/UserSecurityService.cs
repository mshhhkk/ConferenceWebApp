using ConferenceWebApp.Application;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Encodings.Web;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class UserSecurityService : IUserSecurityService
{
    private readonly IEmailSender _emailSender;
    private readonly UserManager<User> _userManager;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ILogger<UserSecurityService> _logger;

    public UserSecurityService(
        UserManager<User> userManager,
         IEmailSender emailSender,
        IUserProfileRepository userProfileRepository,
        ILogger<UserSecurityService> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
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
    public async Task<Result> SendPasswordRecoveryAsync(string email, string baseUrl)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null || !(await _userManager.IsEmailConfirmedAsync(user)))
                return Result.Success(); // делаем вид, что письмо отправили

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Обязательно кодируем токен и параметры
            var link = $"{baseUrl.TrimEnd('/')}/UserSecurity/ResetPassword" +
                       $"?userId={WebUtility.UrlEncode(user.Id.ToString())}" +
                       $"&token={WebUtility.UrlEncode(token)}";

            var html = $@"
                    <p>Вы запросили восстановление пароля для аккаунта <b>{HtmlEncoder.Default.Encode(email)}</b>.</p>
                    <p>Перейдите по ссылке, чтобы задать новый пароль:</p>
                    <p><a href=""{HtmlEncoder.Default.Encode(link)}"">{HtmlEncoder.Default.Encode(link)}</a></p>
                    <p>Если вы не запрашивали восстановление, просто игнорируйте это письмо.</p>";

            await _emailSender.SendAsync(email, "Восстановление пароля", html);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка SendPasswordRecoveryAsync email={Email}", email);
            return Result.Success();
        }
    }

    public async Task<Result> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure("Пользователь не найден.");

            var res = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!res.Succeeded)
            {
                var msg = string.Join("; ", res.Errors.Select(e => $"{e.Code}: {e.Description}"));
                return Result.Failure(msg);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка ResetPasswordAsync userId={UserId}", userId);
            return Result.Failure("Не удалось обновить пароль.");
        }
    }
}
