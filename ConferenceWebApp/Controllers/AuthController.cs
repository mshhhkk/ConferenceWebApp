using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.AuthDTOs;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    // private readonly IUserProfileRepository _userProfileRepository;

    public AuthController(IAuthService authService, IUserProfileRepository userProfileRepository)
        : base(userProfileRepository)
    {
        _authService = authService;
        // _userProfileRepository = userProfileRepository;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDTO dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _authService.RegisterAsync(dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.ErrorMessage);
            return View(dto);
        }

        return View("CheckYourEmail");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var result = await _authService.ConfirmEmailAsync(userId, token);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _authService.SendTwoFactorCodeAsync(dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.ErrorMessage);
            return View(dto);
        }

        TempData["2fa_email"] = dto.Email;
        return RedirectToAction("Verify2FA", new { email = dto.Email });
    }

    [HttpGet]
    public IActionResult Verify2FA(string email) => View(new Verify2FADTO { Email = email });

    [HttpPost]
    public async Task<IActionResult> Verify2FA(Verify2FADTO dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _authService.VerifyTwoFactorCodeAsync(dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.ErrorMessage);
            return View(dto);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }
}