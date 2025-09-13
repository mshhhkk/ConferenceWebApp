using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Infrastructure.Services.Abstract;
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
        IUserProfileRepository userProfileRepository, IPersonalAccountService personalAccountService) : base(userProfileRepository)
    {

        _userManager = userManager;
        _personalAccountService = personalAccountService;
    }



    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        var result = await _personalAccountService.GetEditableProfileAsync(user.Id);
        if (!result.IsSuccess)
        {
            AddError(result.ErrorMessage);
            return RedirectToAction("Index");
        }
        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserDTO dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var result = await _personalAccountService.UpdateProfileAsync(user.Id, dto);

        if (!result.IsSuccess)
        {
            ViewBag.ErrorMessage = result.ErrorMessage;

            return View("Error");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GenerateInvitation()
    {
        // Получаем текущего пользователя
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("Пользователь не найден.");

        var result = await _personalAccountService.GenerateInvitationAsync(user.Id);
        if (!result.IsSuccess)
        {
            ViewBag.ErrorMessage = result.ErrorMessage;
            return View("Error");
        }
        return View(result.Value);
    }
}