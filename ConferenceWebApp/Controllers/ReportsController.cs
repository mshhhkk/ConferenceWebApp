using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ReportsController : BaseController
{
    private readonly IReportService _reportService;
    private readonly UserManager<User> _userManager;
    private readonly IUserProfileService _userProfileService;
    public ReportsController(
        IReportService reportService,
        UserManager<User> userManager,
        IUserProfileService userProfileService) : base(userProfileService)
    {
        _reportService = reportService;
        _userManager = userManager;
        _userProfileService = userProfileService;

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

    public async Task<IActionResult> Index()
    {
        ViewBag.Message = TempData["Error"];
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId!.Value);
        if (!resultUserProfile.IsSuccess)
        {
            TempData["Error"] = resultUserProfile.ErrorMessage;
            return RedirectToAction("Login", "PersonalAccount");
        }

        var reportsResult = await _reportService.GetReportsByUserIdAsync(userId!.Value);
        if (!reportsResult.IsSuccess)
        {
            ViewBag.Message = reportsResult.ErrorMessage;
            return View(new UserReportsViewModel { UserProfile = resultUserProfile.Value, Reports = new List<ReportDTO>() });
        }

        var vm = new UserReportsViewModel
        {
            UserProfile = resultUserProfile.Value,
            Reports = reportsResult.Value
        };

        return View(vm);
    }
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Add(AddReportViewModel vm)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId!.Value);
        if (!resultUserProfile.IsSuccess)
        {
            TempData["Error"] = resultUserProfile.ErrorMessage;
            return RedirectToAction("Login", "PersonalAccount");
        }

        var result = await _reportService.AddReportAsync(vm.Report, userId!.Value);
        if (!result.IsSuccess)
        {
            ViewBag.Message = result.ErrorMessage;
            return View(vm);
        }

        return RedirectToAction("Index");
    }


    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId!.Value);
        if (!resultUserProfile.IsSuccess)
        {
            TempData["Error"] = resultUserProfile.ErrorMessage;
            return RedirectToAction("Login", "PersonalAccount");
        }

        var result = await _reportService.GetReportForEditAsync(id, userId!.Value);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Index");
        }

        var vm = new EditReportViewModel
        {
            UserProfile = resultUserProfile.Value,
            Report = result.Value
        };

        return View(result.Value);
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Edit(EditReportViewModel vm)
    {

        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId!.Value);
        if (!resultUserProfile.IsSuccess)
        {
            TempData["Error"] = resultUserProfile.ErrorMessage;
            return RedirectToAction("Login", "PersonalAccount");
        }

        var result = await _reportService.UpdateReportAsync(vm.Report, userId!.Value);

        if (!result.IsSuccess)
        {
            ViewBag.Message = result.ErrorMessage;
            return View(vm);
        }

        return RedirectToAction("Index");
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId!.Value);
        if (!resultUserProfile.IsSuccess)
        {
            TempData["Error"] = resultUserProfile.ErrorMessage;
            return RedirectToAction("Login", "PersonalAccount");
        }

        var result = await _reportService.DeleteReportAsync(id, userId!.Value);
        if (!result.IsSuccess)
        {
            TempData["Error"] = resultUserProfile.ErrorMessage;
            return RedirectToAction("Index");
        }

        TempData["Success"] = "Доклад успешно удален!";
        return RedirectToAction("Index");
    }


    [HttpGet]
    public async Task<IActionResult> Download(Guid id)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId!.Value);

        if (!resultUserProfile.IsSuccess)
        {
            TempData["Error"] = resultUserProfile.ErrorMessage;
            return RedirectToAction("Login", "PersonalAccount");
        }

        var result = await _reportService.DownloadReportAsync(id, userId!.Value);
        if (!result.IsSuccess)
        {
            TempData["Error"] = resultUserProfile.ErrorMessage;
            return RedirectToAction("Index");
        }


        return result.Value;
    }
}