using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs.AuthDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ITwoFactorService _twoFactorService;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IEmailSender emailSender,
        ITwoFactorService twoFactorService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _twoFactorService = twoFactorService;
    }

    public async Task<Result> RegisterAsync(RegisterDTO dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return Result.Failure("Пользователь с таким email уже существует");

        var user = new User(Guid.NewGuid(), dto.Email);
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "Participant");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"https://yourdomain.com/Auth/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}";
        await _emailSender.SendAsync(dto.Email, "Подтвердите ваш email", $"Для подтверждения перейдите по ссылке: {confirmationLink}");

        return Result.Success();
    }

    public async Task<Result> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure("Пользователь не найден");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            return Result.Failure("Ошибка подтверждения email");

        await _signInManager.SignInAsync(user, isPersistent: true);
        return Result.Success();
    }

    public async Task<Result> SendTwoStepCodeAsync(LoginDTO dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Result.Failure("Неверный логин или пароль");

        var code = _twoFactorService.GenerateCode();
        await _twoFactorService.StoreCodeAsync(dto.Email, code);
        await _emailSender.SendAsync(dto.Email, "Ваш код подтверждения", $"Код: {code}");

        return Result.Success();
    }

    public async Task<Result> VerifyTwoStepsAuthAsync(Verify2SADTO dto)
    {
        if (!await _twoFactorService.ValidateCodeAsync(dto.Email, dto.Code))
            return Result.Failure("Неверный или просроченный код");

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Result.Failure("Пользователь не найден");

        await _signInManager.SignInAsync(user, isPersistent: true);
        return Result.Success();
    }

    public async Task<Result> LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        return Result.Success();
    }
}