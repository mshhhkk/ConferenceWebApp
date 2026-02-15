using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceWebApp.Application.Controllers;

[Authorize]
public class PersonalAccountController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IPersonalAccountService _personalAccountService;
    private readonly ISessionService _sessionService;
    private readonly IAuthService _authService;
    private readonly IInvitationImageService _invitationImageService;
    private readonly IReportLinkingService _linkingService;
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<PersonalAccountController> _logger;

    public PersonalAccountController(
        UserManager<User> userManager,
        IUserProfileService userProfileService,
        IPersonalAccountService personalAccountService,
        IReportLinkingService linkingService,
        ISessionService sessionService,
        IInvitationImageService invitationImageService,
        IAuthService authService,
        ILogger<PersonalAccountController> logger) : base(userProfileService)
    {
        _userManager = userManager;
        _personalAccountService = personalAccountService;
        _sessionService = sessionService;
        _userProfileService = userProfileService;
        _linkingService = linkingService;
        _logger = logger;
        _invitationImageService = invitationImageService;
        _authService = authService;
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
            if (dto.Photo == null && !dto.RemovePhoto)
            {
                var user = await _userManager.GetUserAsync(User);
                dto.PhotoUrl = "~/images/user.svg";
            }
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

        try
        {
            var bind = await _linkingService.BindByNameAsync(userId.Value, dto.LastName, dto.FirstName, dto.MiddleName);
            _logger.LogInformation("Связывание докладов (email={Email}): добавлено={Added}, дублей={Dup}, брак={Bad}",
                bind.Added, bind.SkippedAlreadyExists, bind.SkippedInvalid);

            if (bind.Added > 0)
                TempData["Success"] = $"Подтянули доклады: {bind.Added}.";
        }

        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка авто-связывания докладов по ФИО:", dto.LastName, dto.FirstName, dto.MiddleName);
            TempData["Error"] = "Доклады не найдены, пожалуйста, обратитесь к администратору";
        }
       
        _logger.LogInformation("Профиль успешно обновлён. UserId={UserId}", userId);
        return RedirectToAction("Index", "Reports");
    }

    [HttpGet]
    public async Task<IActionResult> DownloadInvitationPng(CancellationToken ct)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        try
        {
            _logger.LogInformation("Генерация PNG-приглашения. UserId={UserId}", userId);

            var result = await _invitationImageService.BuildForUserAsync(userId!.Value);
            if (!result.IsSuccess || result.Value is null)
            {
                var err = result.ErrorMessage ?? "Не удалось сформировать приглашение.";
                _logger.LogWarning("PNG-приглашение не сгенерировано. UserId={UserId}. {Error}", userId, err);
                TempData["Error"] = err;
                return RedirectToAction("Edit"); // например, в профиль
            }

            var bytes = result.Value.Bytes;

            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            _logger.LogInformation("PNG-приглашение готово. Размер={Len} байт, UserId={UserId}", bytes.Length, userId);

            return File(bytes, "image/png", "Invitation.png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при формировании PNG-приглашения. UserId={UserId}", userId);
            TempData["Error"] = "Произошла ошибка при формировании приглашения.";
            return RedirectToAction("Edit");
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount()
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        try
        {
            _logger.LogInformation("Запрошено удаление аккаунта. UserId={UserId}", userId);

            var result = await _personalAccountService.DeleteAccountAsync(userId!.Value);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Не удалось удалить аккаунт. UserId={UserId}: {Error}", userId, result.ErrorMessage);
                TempData["Error"] = result.ErrorMessage ?? "Не удалось удалить аккаунт.";
                return RedirectToAction(nameof(Edit));
            }
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning("Пользователь не найден при удалении. UserId={UserId}", userId);
                return RedirectToAction("Edit", "PersonalAccount"); 
            }

            var delResult = await _userManager.DeleteAsync(user);
            if (!delResult.Succeeded)
            {
                var err = string.Join("; ", delResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                _logger.LogError("Не удалось удалить пользователя UserId={UserId}. Ошибки: {Err}", userId, err);
                return RedirectToAction("Edit", "PersonalAccount");
            }

            _logger.LogInformation("Аккаунт удалён успешно. UserId={UserId}", userId);
            await _authService.LogoutAsync();
            _sessionService.DeleteSession("UserProfile");

            TempData["Success"] = "Аккаунт успешно удалён.";
            _logger.LogInformation("Аккаунт удалён. UserId={UserId}", userId);
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении аккаунта. UserId={UserId}", userId);
            TempData["Error"] = "Произошла ошибка при удалении аккаунта. Попробуйте позже.";
            return RedirectToAction(nameof(Edit));
        }
    }
}
