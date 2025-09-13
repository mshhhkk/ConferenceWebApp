using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ExtendedThesisController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IExtendedThesisService _thesisService;
    private readonly IUserProfileRepository _userProfileRepository;

    public ExtendedThesisController(
        UserManager<User> userManager,
        IExtendedThesisService thesisService,
        IUserProfileRepository userProfileRepository) : base(userProfileRepository)
    {
        _userManager = userManager;
        _thesisService = thesisService;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var result = await _thesisService.GetExtendedThesisesAsync(user);
        if (!result.IsSuccess)
        {
            ViewBag.ErrorMessage = result.ErrorMessage;
            return View("Error");
        }

        return View(result.Value);
    }

    public async Task<IActionResult> Edit(Guid reportId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var result = await _thesisService.GetThesisAsync(user, reportId);
        if (!result.IsSuccess)
        {
            ViewBag.ErrorMessage = result.ErrorMessage;
            return View("Error");
        }

        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditExtendedThesisViewModel vm)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var resultEdit = await _thesisService.GetThesisAsync(user, vm.Thesis.ReportId);
        if (!resultEdit.IsSuccess)
        {
            ViewBag.ErrorMessage = resultEdit.ErrorMessage;
            return View(vm);
        }

        var result = await _thesisService.UpdateExtendedThesisAsync(user, vm.Thesis);
        if (!result.IsSuccess)
        {
            ViewBag.ErrorMessage = result.ErrorMessage;
            return View(vm);
        }
        return RedirectToAction(nameof(Index));
    }

}