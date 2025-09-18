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
    private readonly UserManager<User> _userManager;

    public UserSecurityController(IUserProfileService userProfileService, IUserSecurityService userSecurityService, UserManager<User> userManager) : base(userProfileService)
    {
        _userSecurityService = userSecurityService;
        _userManager = userManager;
    }

    private async Task<(Guid? userId, IActionResult? redirect)> GetCurrentUserIdAsync()
    {

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return (null, RedirectToAction("Login", "Auth"));
        }
        return (user.Id, null);
    }

    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
    {

        ViewBag.Message = TempData["Error"];
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var result = await _userSecurityService.ChangePasswordAsync(
            userId!.Value,
            model.CurrentPassword,
            model.NewPassword);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("Incorrect password") == true)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Текущий пароль введён неверно.");
            }
            else
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Неизвестная ошибка");
            }
            return View(model);
        }

        TempData["Message"] = "Пароль успешно изменён.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Пользователь не найден.");

        var result = await _userSecurityService.DeleteAccountAsync(user.Id);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage ?? "Не удалось удалить аккаунт";
            return RedirectToAction("Index");
        }

        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        TempData["Message"] = "Ваш аккаунт был успешно удален.";
        return RedirectToAction("Index", "Home");
    }
}