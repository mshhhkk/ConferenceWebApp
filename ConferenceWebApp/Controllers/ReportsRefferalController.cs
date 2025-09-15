using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.ReportsRefferDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ReportsRefferalController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IReportsReferralService _reportsReferralService;
    private readonly IUserProfileService _userProfileService;

    public ReportsRefferalController(
        UserManager<User> userManager,
        IReportsReferralService reportsReferralService,
        IUserProfileService userProfileService) : base(userProfileService)
    {
        _userManager = userManager;
        _reportsReferralService = reportsReferralService;
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
            ViewBag.Message = resultUserProfile.ErrorMessage;
            return View("Error");//TO DO КУДА????
        }

        var resultReports = await _reportsReferralService.GetApprovedReportsForReferral(userId!.Value);

        if (!resultReports.IsSuccess)
        {
            ViewBag.Message = resultReports.ErrorMessage;
            var emptyVm = new ApprovedReportsForReferralViewModel
            {
                UserProfile = resultUserProfile.Value,
                IncomingTransfers = new List<ApprovedReportToRefferalDTO>(),
                Reports = new List<ApprovedReportToRefferalDTO>(),
            };
            return View(emptyVm);
        }

        var vm = new ApprovedReportsForReferralViewModel
        {
            UserProfile = resultUserProfile.Value,
            IncomingTransfers = resultReports.Value.IncomingTransfers,
            Reports = resultReports.Value.Reports
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> RefferSearch(Guid reportId, string? query)
    {
        var model = await _reportsReferralService.SearchUsersForReferral(reportId, query);
        return Json(new { users = model.Users });
    }

    [HttpPost]
    public async Task<IActionResult> Reffer(Guid reportId, Guid targetUserId)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var refferResult = await _reportsReferralService.ReferReport(reportId, targetUserId, userId!.Value);
        if (!refferResult.IsSuccess)
        {
            TempData["Error"] = refferResult.ErrorMessage;
            return RedirectToAction("Index");
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmTransfer(Guid reportId, Guid targetUserId)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var confirmResult = await _reportsReferralService.ConfirmTransfer(reportId, targetUserId);
        if (!confirmResult.IsSuccess)
        {
            TempData["Error"] = confirmResult.ErrorMessage;
            return RedirectToAction("Index");
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> CancelTransfer(Guid reportId)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var cancelResult = await _reportsReferralService.CancelTransfer(reportId, userId!.Value);
        if (!cancelResult.IsSuccess)
        {
            TempData["Error"] = cancelResult.ErrorMessage;
            return RedirectToAction("Index");
        }

        return RedirectToAction("Index");
    }


}


