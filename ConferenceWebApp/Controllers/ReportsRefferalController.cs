using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ReportsRefferalController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IReportsReferralService _reportsReferralService;

    public ReportsRefferalController(
        UserManager<User> userManager,
        IReportsReferralService reportsReferralService,
        IUserProfileRepository userProfileRepository) : base(userProfileRepository)
    {
        _userManager = userManager;
        _reportsReferralService = reportsReferralService;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var dto = await _reportsReferralService.GetApprovedReportsForReferral(currentUser.Id);
        return View(dto);
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
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        await _reportsReferralService.ReferReport(reportId, targetUserId, currentUser.Id);

        TempData["Message"] = "Доклад успешно передан.";
        return RedirectToAction("Index");
    }
    [HttpPost]
    public async Task<IActionResult> ConfirmTransfer(Guid reportId, Guid targetUserId)
    {
        await _reportsReferralService.ConfirmTransfer(reportId, targetUserId);
        TempData["Message"] = "Передача доклада подтверждена.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> CancelTransfer(Guid reportId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        await _reportsReferralService.CancelTransfer(reportId, currentUser.Id);

        TempData["Message"] = "Запрос на передачу доклада отменён.";
        return RedirectToAction("Index");
    }


}


