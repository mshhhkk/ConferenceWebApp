// ReportsController.cs
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ConferenceWebApp.Application;

[Authorize]
public class ReportsController : BaseController
{
    private readonly IReportService _reportService;
    private readonly UserManager<User> _userManager;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserProfileService _userProfileService;
    public ReportsController(
        IReportService reportService,
        UserManager<User> userManager,
        IUserProfileRepository userProfileRepository, IUserProfileService userProfileService) : base(userProfileRepository)
    {
        _reportService = reportService;
        _userManager = userManager;
        _userProfileService = userProfileService;
        _userProfileRepository = userProfileRepository;

    }

    private async Task<Guid> GetCurrentUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.Id ?? throw new InvalidOperationException("Пользователь не аутентифицирован");
    }

    public async Task<IActionResult> Index()
    {
        var userId = await GetCurrentUserIdAsync();

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId);
        if (!resultUserProfile.IsSuccess)
        {
            ViewBag.Message = resultUserProfile.ErrorMessage;
            return View();
        }

        var reportsResult = await _reportService.GetReportsByUserIdAsync(userId);
        if (!reportsResult.IsSuccess)
        {
            ViewBag.Message = reportsResult.ErrorMessage;
            return View();
        }
           
        var vm = new UserReportsViewModel
        {
            UserProfile = resultUserProfile.Value,
            Reports = reportsResult.Value
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddReportViewModel vm)
    {
        var userId = await GetCurrentUserIdAsync();

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId);
        if (!resultUserProfile.IsSuccess)
        {
            ViewBag.Message = resultUserProfile.ErrorMessage;
            return View();
        }

        var result = await _reportService.AddReportAsync(vm.Report, userId);
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
        var userId = await GetCurrentUserIdAsync();

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId);
        if (!resultUserProfile.IsSuccess)
        {
            ViewBag.Message = resultUserProfile.ErrorMessage;
            return RedirectToAction("Index"); ;
        }

        var result = await _reportService.GetReportForEditAsync(id, userId);

        if (!result.IsSuccess)
        {
            ViewBag.Message = result.ErrorMessage;
            return RedirectToAction("Index");
        }

        var vm = new EditReportViewModel
        {
            UserProfile = resultUserProfile.Value,
            Report = result.Value
        };

        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditReportViewModel vm)
    {

        var userId = await GetCurrentUserIdAsync();

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId);
        if (!resultUserProfile.IsSuccess)
        {
            ViewBag.Message = resultUserProfile.ErrorMessage;
            return RedirectToAction("Index"); ;
        }

        var result = await _reportService.UpdateReportAsync(vm.Report, userId);

        if (!result.IsSuccess)
        {
            if (!result.IsSuccess)
            {
                ViewBag.Message = result.ErrorMessage;
                return View(vm);
            }
        }

        return RedirectToAction("Index");
    }


    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken] 
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var userId = await GetCurrentUserIdAsync();

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId);
        if (!resultUserProfile.IsSuccess)
        {
            ViewBag.Message = resultUserProfile.ErrorMessage;
            return RedirectToAction("Index"); ;
        }

        var result = await _reportService.DeleteReportAsync(id, userId);

        if (!result.IsSuccess)
            return View("Error", result.ErrorMessage);

        return RedirectToAction("Index");
    }


    [HttpGet]
    public async Task<IActionResult> Download(Guid id)
    {
        var userId = await GetCurrentUserIdAsync();
        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId);

        if (!resultUserProfile.IsSuccess)
        {
            ViewBag.Message = resultUserProfile.ErrorMessage;
            return RedirectToAction("Edit"); 
        }

        var result = await _reportService.DownloadReportAsync(id, userId);

        if (!result.IsSuccess)
            return NotFound();

        return result.Value;
    }
}