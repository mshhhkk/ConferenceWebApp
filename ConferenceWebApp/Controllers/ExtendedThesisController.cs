using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ExtendedThesisController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IExtendedThesisService _thesisService;
    private readonly IUserProfileService _userProfileService;

    public ExtendedThesisController(
        UserManager<User> userManager,
        IExtendedThesisService thesisService,
        IUserProfileService userProfileService) : base(userProfileService)
    {
        _userManager = userManager;
        _thesisService = thesisService;
        _userProfileService = userProfileService;
        _userProfileService = userProfileService;
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
    public async Task<IActionResult> Index()
    {

        ViewBag.Message = TempData["Error"];
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId!.Value);
        if (!resultUserProfile.IsSuccess)
        {
            ViewBag.Message = resultUserProfile.ErrorMessage;
            return View(new ExtendedThesisViewModel());
        }

        var resultExtThesis = await _thesisService.GetExtendedThesisesAsync(userId!.Value);

        if (!resultExtThesis.IsSuccess)
        {
            ViewBag.ErrorMessage = resultExtThesis.ErrorMessage;
            return View(new ExtendedThesisViewModel());
        }

        var vm = new ExtendedThesisViewModel
        {
            UserProfile = resultUserProfile.Value,
            ReportsWithTheses = resultExtThesis.Value.ReportsWithTheses,
            ReportsWithoutTheses = resultExtThesis.Value.ReportsWithoutTheses
        };
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid reportId)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var result = await _thesisService.GetThesisAsync(userId!.Value, reportId);
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
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var resultEdit = await _thesisService.GetThesisAsync(userId!.Value, vm.Thesis.ReportId);
        if (!resultEdit.IsSuccess)
        {
            ViewBag.ErrorMessage = resultEdit.ErrorMessage;
            return View(vm);
        }

        var result = await _thesisService.UpdateExtendedThesisAsync(userId!.Value, vm.Thesis);
        if (!result.IsSuccess)
        {
            ViewBag.ErrorMessage = result.ErrorMessage;
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }
}
