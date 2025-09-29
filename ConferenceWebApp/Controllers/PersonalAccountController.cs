using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Application.Controllers;

[Authorize]
public class PersonalAccountController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IPersonalAccountService _personalAccountService;
    private readonly ISessionService _sessionService;
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<PersonalAccountController> _logger;

    public PersonalAccountController(
        UserManager<User> userManager,
        IUserProfileService userProfileService,
        IPersonalAccountService personalAccountService,
        ISessionService sessionService,
        ILogger<PersonalAccountController> logger) : base(userProfileService)
    {
        _userManager = userManager;
        _personalAccountService = personalAccountService;
        _sessionService = sessionService;
        _userProfileService = userProfileService;
        _logger = logger;
    }

    private async Task<(Guid? userId, IActionResult? redirect)> GetCurrentUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("Неавторизованный доступ к PersonalAccount.*");
            return (null, RedirectToAction("Login", "Auth"));
        }
        return (user.Id, null);
    }

    public async Task<IActionResult> Edit()
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Открыт профиль для редактирования. UserId={UserId}", userId);

        var result = await _personalAccountService.GetProfileToEditByUserIdAsync(userId!.Value);
        if (!result.IsSuccess)
        {
            _logger.LogError("Не удалось получить профиль для редактирования. UserId={UserId}: {Error}",
                userId, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Login", "Auth");
        }

        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserDTO dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Невалидная модель при сохранении профиля.");
            return View(dto);
        }

        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Сохранение профиля. UserId={UserId}", userId);

        var result = await _personalAccountService.UpdateProfileAsync(userId!.Value, dto);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Не удалось обновить профиль. UserId={UserId}: {Error}",
                userId, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage;
            return View(dto);
        }

        var userProfile = await _userProfileService.GetByUserIdAsync(userId!.Value);
        if (!userProfile.IsSuccess)
        {
            _logger.LogError("Профиль обновлён, но не удалось перечитать UserProfile. UserId={UserId}: {Error}",
                userId, userProfile.ErrorMessage);
        }
        else
        {
            _sessionService.UpdateSession("UserProfile", userProfile.Value);
        }

        _logger.LogInformation("Профиль успешно обновлён. UserId={UserId}", userId);
        return RedirectToAction("Index", "Reports");
    }

    [HttpGet]
    public async Task<IActionResult> GenerateInvitation()
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Генерация приглашения. UserId={UserId}", userId);

        var result = await _personalAccountService.GenerateInvitationAsync(userId!.Value);
        if (!result.IsSuccess)
        {
            _logger.LogError("Ошибка генерации приглашения. UserId={UserId}: {Error}",
                userId, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Login", "Auth");
        }

        _logger.LogInformation("Приглашение сгенерировано. UserId={UserId}", userId);
        return View(result.Value);
    }
}
