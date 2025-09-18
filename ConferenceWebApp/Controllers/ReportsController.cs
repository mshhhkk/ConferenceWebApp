using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Application.Validation;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[Authorize]
public class ReportsController : BaseController
{
    private readonly IReportService _reportService;
    private readonly UserManager<User> _userManager;
    private readonly IUserProfileService _userProfileService;
    private readonly ISessionService _sessionService;  // Сервис сессий

    public ReportsController(
        IReportService reportService,
        UserManager<User> userManager,
        IUserProfileService userProfileService,
        ISessionService sessionService) : base(userProfileService)
    {
        _reportService = reportService;
        _userManager = userManager;
        _userProfileService = userProfileService;
        _sessionService = sessionService;
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

        var userProfileJson = HttpContext.Session.GetString("UserProfile");
        if (string.IsNullOrEmpty(userProfileJson))
        {
            return RedirectToAction("Login", "Auth");
        }

        var userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJson);

        if (userProfile == null)
        {
            TempData["Error"] = "Не удалось загрузить профиль пользователя.";
            return RedirectToAction("Login", "Auth");
        }

        var reportsResult = await _reportService.GetReportsByUserIdAsync(userId!.Value);
        if (!reportsResult.IsSuccess)
        {
            ViewBag.Message = reportsResult.ErrorMessage;
            return View(new UserReportsViewModel { UserProfile = userProfile, Reports = new List<ReportDTO>() });
        }

        var vm = new UserReportsViewModel
        {
            UserProfile = userProfile,
            Reports = reportsResult.Value
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var userProfileJson = HttpContext.Session.GetString("UserProfile");
        if (string.IsNullOrEmpty(userProfileJson))
        {
            return RedirectToAction("Login", "Auth");
        }

        var userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJson);

        return View(new AddReportViewModel { UserProfile = userProfile, Report = new AddReportDTO() });
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Add(AddReportViewModel vm)
    {
        var validator = new AddReportValidator();

        var userProfileJson = HttpContext.Session.GetString("UserProfile");

        if (string.IsNullOrEmpty(userProfileJson))
        {
            return RedirectToAction("Login", "Auth");
        }

        var userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJson);
        var validationResult = validator.Validate(vm.Report);
        if (!validationResult.IsValid)
        {
            vm.UserProfile = userProfile;
            return View(vm);
        }

        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

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

        var userProfileJson = HttpContext.Session.GetString("UserProfile");
        if (string.IsNullOrEmpty(userProfileJson))
        {
            return RedirectToAction("Login", "Auth");
        }

        var userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJson);

        var result = await _reportService.GetReportForEditAsync(id, userId!.Value);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Index");
        }

        var vm = new EditReportViewModel
        {
            UserProfile = userProfile,
            Report = result.Value
        };

        return View(vm);
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Edit(EditReportViewModel vm)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var userProfileJson = HttpContext.Session.GetString("UserProfile");

        if (string.IsNullOrEmpty(userProfileJson))
        {
            return RedirectToAction("Login", "Auth");
        }

        var validator = new EditReportValidator();
        var validationResult = validator.Validate(vm.Report);

        if (!validationResult.IsValid)
        {
            var userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJson);
            vm.UserProfile = userProfile;
            return View(vm);
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

        var userProfileJson = HttpContext.Session.GetString("UserProfile");
        if (string.IsNullOrEmpty(userProfileJson))
        {
            return RedirectToAction("Login", "Auth");
        }

        var userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJson);

        var result = await _reportService.DeleteReportAsync(id, userId!.Value);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
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

        var userProfileJson = HttpContext.Session.GetString("UserProfile");
        if (string.IsNullOrEmpty(userProfileJson))
        {
            return RedirectToAction("Login", "Auth");
        }

        var userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJson);

        var result = await _reportService.DownloadReportAsync(id, userId!.Value);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Index");
        }

        return result.Value;
    }
}
