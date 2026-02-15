using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.AuthDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly ISessionService _sessionService;
    private readonly IUserProfileService _userProfileService;
    private readonly IReportLinkingService _linkingService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IUserProfileService userProfileService,
        ISessionService sessionService,
        UserManager<User> userManager,
        IReportLinkingService linkingService,
        ILogger<AuthController> logger)
        : base(userProfileService)
    {
        _authService = authService;
        _sessionService = sessionService;
        _userProfileService = userProfileService;
        _linkingService = linkingService;
        _userManager = userManager;
        _logger = logger;
    }

    public IActionResult Login() => View();
    public IActionResult Register() => View();

    public IActionResult Verify2SA() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
    LoginDTO dto,
    [FromServices] FluentValidation.IValidator<LoginDTO> validator)
    {
        var vr = await validator.ValidateAsync(dto);
        if (!vr.IsValid)
        {
            foreach (var e in vr.Errors)
                ModelState.AddModelError(e.PropertyName, e.ErrorMessage);

            _logger.LogWarning("Попытка логина с невалидной моделью. Email={Email}", dto.Email);
            
            return View(dto);
        }

        var result = await _authService.SendTwoStepCodeAsync(dto);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Ошибка логина для {Email}: {Error}", dto.Email, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage;
            return View(dto);
        }

        _logger.LogInformation("Пользователь {Email} успешно прошёл первый этап логина", dto.Email);
        TempData["2fa_email"] = dto.Email;
        return RedirectToAction("Verify2SA", new { email = dto.Email });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
    RegisterDTO dto,
    [FromServices] FluentValidation.IValidator<RegisterDTO> validator)
    {
        var vr = await validator.ValidateAsync(dto);
        if (!vr.IsValid)
        {
            foreach (var e in vr.Errors)
                ModelState.AddModelError(e.PropertyName, e.ErrorMessage);

            _logger.LogWarning("Попытка регистрации с невалидной моделью (FluentValidation). Email={Email}", dto.Email);
            return View(dto);
        }

        try
        {
            _logger.LogInformation("Старт регистрации пользователя. Email={Email}", dto.Email);

            var result = await _authService.RegisterAsync(dto);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Ошибка регистрации для {Email}: {Error}", dto.Email, result.ErrorMessage);
                ModelState.AddModelError(string.Empty, result.ErrorMessage!);
                return View(dto);
            }

            _logger.LogInformation("Пользователь {Email} успешно зарегистрирован. Отправлено письмо подтверждения.", dto.Email);
            return View("CheckYourEmail");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка при регистрации пользователя {Email}", dto.Email);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при регистрации. Попробуйте позже.");
            return View(dto);
        }
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
        var userProfileResult = await _userProfileService.GetByUserIdAsync(user!.Id);

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

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var result = await _authService.ConfirmEmailAsync(userId, token);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        var user = await _userManager.GetUserAsync(User);
        try
        {
            var bind = await _linkingService.BindByEmailAsync(user.Id, user.Email);
            _logger.LogInformation("Связывание докладов (email={Email}): добавлено={Added}, дублей={Dup}, брак={Bad}",
                user.Email, bind.Added, bind.SkippedAlreadyExists, bind.SkippedInvalid);

            if (bind.Added > 0)
                TempData["Success"] = $"Подтянули доклады: {bind.Added}. Пожалуйста, заполните личный профиль. ";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка авто-связывания докладов по email={Email}", user.Email);
            TempData["Error"] = "Доклады не найдены, пожалуйста, заполните личную информацию";
        }
        var userProfileResult = await _userProfileService.GetByUserIdAsync(user!.Id);
        _sessionService.SaveSession("UserProfile", userProfileResult.Value);
        return RedirectToAction("Edit", "PersonalAccount");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation([FromForm] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] = "Укажите e-mail.";
            return RedirectToAction("Register");
        }

        var res = await _authService.ResendConfirmationEmailAsync(email);
        if (!res.IsSuccess)
        {
            TempData["Error"] = res.ErrorMessage ?? "Не удалось отправить письмо.";
            return RedirectToAction("Register");
        }

        TempData["Success"] = "Письмо с подтверждением отправлено.";
        return RedirectToAction("CheckYourEmail");
    }
}

