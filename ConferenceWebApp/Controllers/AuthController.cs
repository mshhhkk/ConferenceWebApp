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

    public AuthController(IAuthService authService, IUserProfileService userProfileService, ISessionService sessionService, UserManager<User> userManager)
         : base(userProfileService)
    {
        _authService = authService;
        _sessionService = sessionService;
        _userProfileService = userProfileService;
        _userManager = userManager;
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
        var userProfileResult = await _userProfileService.GetByUserIdAsync(Guid.Parse(userId));
        if (!userProfileResult.IsSuccess)
        {
            TempData["Error"] = userProfileResult.ErrorMessage;
            return View();
        }

        _sessionService.SaveSession("UserProfile", userProfileResult.Value);

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

        var user = await _userManager.GetUserAsync(User);
        var userProfileResult = await _userProfileService.GetByUserIdAsync(user.Id);
        if (!userProfileResult.IsSuccess)
        {
            TempData["Error"] = userProfileResult.ErrorMessage;
            return View();
        }

        _sessionService.SaveSession("UserProfile", userProfileResult.Value);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        _sessionService.DeleteSession("UserProfile");
        return RedirectToAction("Index", "Home");
    }
}
