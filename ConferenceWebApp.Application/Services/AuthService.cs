using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs.AuthDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IUserProfileService _userProfileService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger; 

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUserProfileService userProfileService,
        IHttpContextAccessor httpContextAccessor,
        ITwoFactorService twoFactorService,
        IEmailSender emailSender,
        ILogger<AuthService> logger) 
    {
        _twoFactorService = twoFactorService;
        _userManager = userManager;
        _signInManager = signInManager;
        _userProfileService = userProfileService;
        _httpContextAccessor = httpContextAccessor;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Result> RegisterAsync(RegisterDTO dto)
    {
        _logger.LogInformation("Попытка регистрации пользователя {Email}", dto.Email);

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Регистрация отклонена: пользователь {Email} уже существует", dto.Email);
            return Result.Failure("Пользователь с таким email уже существует");
        }

        var user = new User(Guid.NewGuid(), dto.Email);
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var error = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Ошибка при создании пользователя {Email}: {Error}", dto.Email, error);
            return Result.Failure(error);
        }

        await _userManager.AddToRoleAsync(user, "Participant");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"https://yourdomain.com/Auth/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        await _emailSender.SendAsync(dto.Email, "Подтвердите ваш email",
            $"Для подтверждения перейдите по ссылке: {confirmationLink}");

        _logger.LogInformation("Пользователь {Email} успешно зарегистрирован. Отправлено письмо с подтверждением", dto.Email);

        return Result.Success();
    }

    public async Task<Result> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Не найден пользователь для подтверждения email. Id={UserId}", userId);
            return Result.Failure("Пользователь не найден");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            _logger.LogError("Ошибка подтверждения email для {Email}", user.Email);
            return Result.Failure("Ошибка подтверждения email");
        }

        await _signInManager.SignInAsync(user, isPersistent: true);
        _logger.LogInformation("Email подтверждён и пользователь {Email} автоматически вошёл в систему", user.Email);

        return Result.Success();
    }

    public async Task<Result> SendTwoStepCodeAsync(LoginDTO dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            _logger.LogWarning("Неуспешный вход: неверный логин или пароль {Email}", dto.Email);
            return Result.Failure("Неверный логин или пароль");
        }

        var code = _twoFactorService.GenerateCode();
        await _twoFactorService.StoreCodeAsync(dto.Email, code);
        await _emailSender.SendAsync(dto.Email, "Ваш код подтверждения", $"Код: {code}");

        _logger.LogInformation("Пользователю {Email} отправлен 2FA-код", dto.Email);
        return Result.Success();
    }

    public async Task<Result> VerifyTwoStepsAuthAsync(Verify2SADTO dto)
    {
        if (!await _twoFactorService.ValidateCodeAsync(dto.Email, dto.Code))
        {
            _logger.LogWarning("Неуспешная проверка 2FA-кода для {Email}", dto.Email);
            return Result.Failure("Неверный или просроченный код");
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            _logger.LogError("2FA: пользователь не найден {Email}", dto.Email);
            return Result.Failure("Пользователь не найден");
        }

        var userProfileResult = await _userProfileService.GetByUserIdAsync(user.Id);
        if (!userProfileResult.IsSuccess)
        {
            _logger.LogError("2FA: не удалось получить профиль для {Email}", dto.Email);
            return Result.Failure("Не удалось получить профиль пользователя.");
        }

        await _signInManager.SignInAsync(user, isPersistent: true);
        _logger.LogInformation("Пользователь {Email} успешно прошёл 2FA", dto.Email);

        _httpContextAccessor.HttpContext.Session.SetString("UserProfile",
            JsonConvert.SerializeObject(userProfileResult.Value));

        return Result.Success();
    }

    public async Task<Result> LogoutAsync()
    {
        var email = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "Anonymous";
        await _signInManager.SignOutAsync();
        _httpContextAccessor.HttpContext?.Session.Clear();

        _logger.LogInformation("Пользователь {Email} вышел из системы", email);
        return Result.Success();
    }
}
