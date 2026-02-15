using ConferenceWebApp.Application.DTOs;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceWebApp.Application.Controllers;

public class UserSecurityController : BaseController
{
    private readonly IUserSecurityService _userSecurityService;
    private readonly IUserProfileService _userProfileService;
    private readonly UserManager<User> _userManager;          
    private readonly IConfiguration _cfg;
    private readonly ILogger<UserSecurityController> _logger;

    public UserSecurityController(
        IUserProfileService userProfileService,
        IUserSecurityService userSecurityService,
        UserManager<User> userManager,                     
        IConfiguration cfg,
        ILogger<UserSecurityController> logger)
        : base(userProfileService)
    {
        _userSecurityService = userSecurityService;
        _userProfileService = userProfileService;
        _userManager = userManager;                         
        _cfg = cfg;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult ChangePassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);       // ← используем UserManager
        if (user is null)
        {
            _logger.LogWarning("ChangePassword: user is null (not authenticated)");
            return RedirectToAction("Login", "Auth");
        }

        var res = await _userSecurityService.ChangePasswordAsync(user.Id, model.CurrentPassword, model.NewPassword);
        if (!res.IsSuccess)
        {
            if ((res.ErrorMessage ?? "").Contains("Incorrect password", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError(nameof(model.CurrentPassword), "Текущий пароль введён неверно.");
            else
                ModelState.AddModelError(string.Empty, res.ErrorMessage ?? "Не удалось сменить пароль.");

            return View(model);
        }

        TempData["Success"] = "Пароль успешно изменён.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult PasswordRecovery() => View(new PasswordRecoveryDTO());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasswordRecovery(PasswordRecoveryDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var baseUrl = _cfg["Urls:PublicBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            baseUrl = $"{Request.Scheme}://{Request.Host}";

        var res = await _userSecurityService.SendPasswordRecoveryAsync(dto.Email, baseUrl);
        if (!res.IsSuccess)
        {
            _logger.LogWarning("PasswordRecovery fail for {Email}: {Err}", dto.Email, res.ErrorMessage);
        }

        TempData["Success"] = "Если такой email существует и подтверждён, мы отправили ссылку для сброса пароля.";
        return RedirectToAction("Login", "Auth");
    }

    [HttpGet]
    public IActionResult ResetPassword(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return BadRequest("Некорректная ссылка для сброса пароля.");

        return View(new ResetPasswordDTO { UserId = userId, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var res = await _userSecurityService.ResetPasswordAsync(dto.UserId, dto.Token, dto.NewPassword);
        if (!res.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, res.ErrorMessage ?? "Не удалось обновить пароль.");
            return View(dto);
        }

        TempData["Success"] = "Пароль успешно обновлён. Войдите с новым паролем.";
        return RedirectToAction("Login", "Auth");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User); 
        if (user is null)
        {
            _logger.LogWarning("DeleteAccount: user is null (not authenticated)");
            return RedirectToAction("Login", "Auth");
        }

        var res = await _userSecurityService.DeleteAccountAsync(user.Id);
        if (!res.IsSuccess)
        {
            TempData["Error"] = res.ErrorMessage ?? "Не удалось удалить аккаунт.";
            return RedirectToAction("Index", "Home");
        }

        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        TempData["Success"] = "Аккаунт удалён.";
        return RedirectToAction("Index", "Home");
    }
}
