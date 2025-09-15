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

    public PersonalAccountController(
        UserManager<User> userManager,
        IUserProfileService userProfileService, IPersonalAccountService personalAccountService) : base(userProfileService)
    {

        _userManager = userManager;
        _personalAccountService = personalAccountService;
    }

    private async Task<(Guid? userId, IActionResult? redirect)> GetCurrentUserIdAsync()
    {

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return (null, RedirectToAction("Login", "PersonalAccount"));
        }
        return (user.Id, null);
    }

    public async Task<IActionResult> Edit()
    {
        ViewBag.Message = TempData["Error"];

        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var result = await _personalAccountService.GetProfileToEditByUserIdAsync(userId!.Value);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Login", "PersonalAccount");
        }

        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserDTO dto)
    {

        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var result = await _personalAccountService.UpdateProfileAsync(userId!.Value, dto);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return View();
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> GenerateInvitation()
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var result = await _personalAccountService.GenerateInvitationAsync(userId!.Value);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Login", "PersonalAccount");
        }
        return View(result.Value);
    }
}
