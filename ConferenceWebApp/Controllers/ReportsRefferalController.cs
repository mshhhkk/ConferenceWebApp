using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.ReportsRefferDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[Authorize]
public class ReportsRefferalController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IReportsReferralService _reportsReferralService;
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<ReportsRefferalController> _logger;

    public ReportsRefferalController(
        UserManager<User> userManager,
        IReportsReferralService reportsReferralService,
        IUserProfileService userProfileService,
        ILogger<ReportsRefferalController> logger) : base(userProfileService)
    {
        _userManager = userManager;
        _reportsReferralService = reportsReferralService;
        _userProfileService = userProfileService;
        _logger = logger;
    }

    private async Task<(Guid? userId, IActionResult? redirect)> GetCurrentUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("Неавторизованный доступ к ReportsRefferal.*");
            return (null, RedirectToAction("Login", "Auth"));
        }
        return (user.Id, null);
    }

    public async Task<IActionResult> Index()
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Открыта страница передачи докладов. UserId={UserId}", userId);

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId!.Value);
        if (!resultUserProfile.IsSuccess)
        {
            _logger.LogError("Не удалось получить профиль пользователя. UserId={UserId}: {Error}",
                userId, resultUserProfile.ErrorMessage);

            ViewBag.Message = resultUserProfile.ErrorMessage;
            return View("Error");
        }

        var resultReports = await _reportsReferralService.GetApprovedReportsForReferral(userId!.Value);
        if (!resultReports.IsSuccess)
        {
            _logger.LogWarning("Не удалось получить список докладов для передачи. UserId={UserId}: {Error}",
                userId, resultReports.ErrorMessage);

            var emptyVm = new ApprovedReportsForReferralViewModel
            {
                UserProfile = resultUserProfile.Value,
                IncomingTransfers = new List<ApprovedReportToRefferalDTO>(),
                Reports = new List<ApprovedReportToRefferalDTO>(),
            };
            ViewBag.Message = resultReports.ErrorMessage;
            return View(emptyVm);
        }

        _logger.LogInformation("Получено: входящих переносов={Incoming}, доступных к передаче={Reports}. UserId={UserId}",
            resultReports.Value.IncomingTransfers?.Count ?? 0,
            resultReports.Value.Reports?.Count ?? 0,
            userId);

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
        _logger.LogInformation("Поиск пользователей для передачи доклада. ReportId={ReportId}, Query={Query}",
            reportId, query);

        var model = await _reportsReferralService.SearchUsersForReferral(reportId, query);
        return Json(new { users = model.Users });
    }

    [HttpPost]
    public async Task<IActionResult> Reffer(Guid reportId, Guid targetUserId)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Попытка передачи доклада. ReportId={ReportId}, FromUserId={From}, ToUserId={To}",
            reportId, userId, targetUserId);

        var refferResult = await _reportsReferralService.ReferReport(reportId, targetUserId, userId!.Value);
        if (!refferResult.IsSuccess)
        {
            _logger.LogWarning("Передача доклада не удалась. ReportId={ReportId}, From={From}, To={To}: {Error}",
                reportId, userId, targetUserId, refferResult.ErrorMessage);
            TempData["Error"] = refferResult.ErrorMessage;
            return RedirectToAction("Index");
        }

        _logger.LogInformation("Доклад отправлен на передачу. ReportId={ReportId}, From={From}, To={To}", reportId, userId, targetUserId);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmTransfer(Guid reportId, Guid targetUserId)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Подтверждение передачи доклада. ReportId={ReportId}, TargetUserId={Target}", reportId, targetUserId);

        var confirmResult = await _reportsReferralService.ConfirmTransfer(reportId, targetUserId);
        if (!confirmResult.IsSuccess)
        {
            _logger.LogWarning("Подтверждение передачи не удалось. ReportId={ReportId}, Target={Target}: {Error}",
                reportId, targetUserId, confirmResult.ErrorMessage);
            TempData["Error"] = confirmResult.ErrorMessage;
            return RedirectToAction("Index");
        }

        _logger.LogInformation("Передача доклада подтверждена. ReportId={ReportId}, Target={Target}", reportId, targetUserId);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> CancelTransfer(Guid reportId)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Отмена передачи доклада. ReportId={ReportId}, ByUserId={UserId}", reportId, userId);

        var cancelResult = await _reportsReferralService.CancelTransfer(reportId, userId!.Value);
        if (!cancelResult.IsSuccess)
        {
            _logger.LogWarning("Отмена передачи не удалась. ReportId={ReportId}, ByUserId={UserId}: {Error}",
                reportId, userId, cancelResult.ErrorMessage);
            TempData["Error"] = cancelResult.ErrorMessage;
            return RedirectToAction("Index");
        }

        _logger.LogInformation("Передача доклада отменена. ReportId={ReportId}, ByUserId={UserId}", reportId, userId);
        return RedirectToAction("Index");
    }
}
