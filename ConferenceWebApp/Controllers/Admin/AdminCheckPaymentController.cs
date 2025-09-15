using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Application.Interfaces.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminCheckPaymentController : BaseController
{
    private readonly IAdminPaymentService _adminPaymentService;

    public AdminCheckPaymentController(
        IUserProfileService userProfileService,
        IAdminPaymentService adminPaymentService)
        : base(userProfileService)
    {
        _adminPaymentService = adminPaymentService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _adminPaymentService.GetUsersWithReceiptsAsync();

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(new List<UserWithReceiptDTO>());
        }

        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsPaid(Guid userId)
    {
        var result = await _adminPaymentService.MarkPaymentAsPaidAsync(userId);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
        }
        else
        {
            TempData["Message"] = "Оплата успешно подтверждена.";
        }

        return RedirectToAction(nameof(Index));
    }
}