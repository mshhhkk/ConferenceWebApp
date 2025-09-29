using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.AuthDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly ISessionService _sessionService;
    private readonly IUserProfileService _userProfileService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthController> _logger; 

    public AuthController(
        IAuthService authService,
        IUserProfileService userProfileService,
        ISessionService sessionService,
        UserManager<User> userManager,
        ILogger<AuthController> logger)
        : base(userProfileService)
    {
        _authService = authService;
        _sessionService = sessionService;
        _userProfileService = userProfileService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Попытка логина с невалидной моделью. Email={Email}", dto.Email);
            return View(dto);
        }

        var result = await _authService.SendTwoStepCodeAsync(dto);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Ошибка логина для {Email}: {Error}", dto.Email, result.ErrorMessage);
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return View(dto);
        }

        _logger.LogInformation("Пользователь {Email} успешно прошёл первый этап логина", dto.Email);
        TempData["2fa_email"] = dto.Email;
        return RedirectToAction("Verify2SA", new { email = dto.Email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify2SA(Verify2SADTO dto)
    {
        var result = await _authService.VerifyTwoStepsAuthAsync(dto);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Ошибка 2FA для {Email}: {Error}", dto.Email, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Index");
        }

        _logger.LogInformation("Пользователь {Email} успешно вошёл в систему", dto.Email);
        var user = await _userManager.GetUserAsync(User);
        var userProfileResult = await _userProfileService.GetByUserIdAsync(user.Id);

        if (!userProfileResult.IsSuccess)
        {
            _logger.LogError("Не удалось загрузить профиль пользователя {Email}", dto.Email);
            TempData["Error"] = userProfileResult.ErrorMessage;
            return View();
        }

        _sessionService.SaveSession("UserProfile", userProfileResult.Value);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var email = User.Identity?.Name ?? "Anonymous";
        await _authService.LogoutAsync();
        _sessionService.DeleteSession("UserProfile");

        _logger.LogInformation("Пользователь {Email} вышел из системы", email);
        return RedirectToAction("Index", "Home");
    }
}
