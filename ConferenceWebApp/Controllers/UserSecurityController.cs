using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Application.Controllers;

public class UserSecurityController : BaseController
{
    private readonly IUserSecurityService _userSecurityService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UserSecurityController> _logger;

    public UserSecurityController(
        IUserProfileService userProfileService,
        IUserSecurityService userSecurityService,
        UserManager<User> userManager,
        ILogger<UserSecurityController> logger)
        : base(userProfileService)
    {
        _userSecurityService = userSecurityService;
        _userManager = userManager;
        _logger = logger;
    }

    private async Task<(Guid? userId, IActionResult? redirect)> GetCurrentUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("Попытка доступа без авторизации в {Controller}", nameof(UserSecurityController));
            return (null, RedirectToAction("Login", "Auth"));
        }
        return (user.Id, null);
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        _logger.LogInformation("Открыта страница смены пароля пользователем {User}", User.Identity?.Name);
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Модель смены пароля некорректна");
            return View(model);
        }

        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Пользователь {UserId} инициировал смену пароля", userId);

        var result = await _userSecurityService.ChangePasswordAsync(
            userId!.Value,
            model.CurrentPassword,
            model.NewPassword);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Ошибка смены пароля для {UserId}: {Error}", userId, result.ErrorMessage);

            if (result.ErrorMessage?.Contains("Incorrect password") == true)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Текущий пароль введён неверно.");
            }
            else
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Неизвестная ошибка");
            }
            return View(model);
        }

        TempData["Message"] = "Пароль успешно изменён.";
        _logger.LogInformation("Пользователь {UserId} успешно сменил пароль", userId);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogError("Не удалось найти пользователя при удалении аккаунта");
            return NotFound("Пользователь не найден.");
        }

        _logger.LogInformation("Пользователь {UserId} инициировал удаление аккаунта", user.Id);

        var result = await _userSecurityService.DeleteAccountAsync(user.Id);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Ошибка удаления аккаунта {UserId}: {Error}", user.Id, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage ?? "Не удалось удалить аккаунт";
            return RedirectToAction("Index");
        }

        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        TempData["Message"] = "Ваш аккаунт был успешно удален.";
        _logger.LogInformation("Пользователь {UserId} удалил аккаунт", user.Id);

        return RedirectToAction("Index", "Home");
    }
}
