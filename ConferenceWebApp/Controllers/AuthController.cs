using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.AuthDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService, IUserProfileService userProfileService)
        : base(userProfileService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        ViewBag.Error = TempData["Error"];
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Register(RegisterDTO dto)
    {

        var result = await _authService.RegisterAsync(dto);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return View();
        }

        return View("CheckYourEmail");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var result = await _authService.ConfirmEmailAsync(userId, token);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return View();
        }

        return RedirectToAction("Edit", "PersonalAccount");
    }

    [HttpGet]
    public IActionResult Login()
    {
        ViewBag.Error = TempData["Error"];
        return View();
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Login(LoginDTO dto)
    {

        var result = await _authService.SendTwoStepCodeAsync(dto);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return View();
        }

        TempData["2fa_email"] = dto.Email;
        return RedirectToAction("Verify2SA", new { email = dto.Email });
    }

    [HttpGet]
    public IActionResult Verify2SA(string email) => View(new Verify2SADTO { Email = email });

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Verify2SA(Verify2SADTO dto)
    {

        var result = await _authService.VerifyTwoStepsAuthAsync(dto);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Index"); ;
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
