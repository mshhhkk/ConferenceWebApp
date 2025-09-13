// ReportsController.cs
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ReportsController : BaseController
{
    private readonly IReportService _reportService;
    private readonly UserManager<User> _userManager;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserService _userService;
    public ReportsController(
        IReportService reportService,
        UserManager<User> userManager,
        IUserProfileRepository userProfileRepository) : base(userProfileRepository)
    {
        _reportService = reportService;
        _userManager = userManager;
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
        var result = await _reportService.GetUserReportsAsync(userId);

        if (!result.IsSuccess)
            return View("Error", result.ErrorMessage);

        return View(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        var userId = await GetCurrentUserIdAsync();
        var result = await _reportService.GetUserProfileForAddingReportsAsync(userId);
        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddReportViewModel vm)
    {
        var userIdPost = await GetCurrentUserIdAsync();
        var result = await _reportService.CreateReportAsync(vm.Report, userIdPost);

        if (!result.IsSuccess)
        {
            // Получаем профиль пользователя
            var profileResult = await _reportService.GetUserProfileForAddingReportsAsync(userIdPost);

            if (profileResult.IsSuccess)
            {
                vm.UserProfile = profileResult.Value.UserProfile;
            }
            ModelState.AddModelError("", result.ErrorMessage);
            return View(vm);
        }

        return RedirectToAction("Index");
    }


    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var userId = await GetCurrentUserIdAsync();
        var result = await _reportService.GetReportForEditAsync(id, userId);

        if (!result.IsSuccess)
            return View("Error", result.ErrorMessage);

        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditReportViewModel vm)
    {

        var userId = await GetCurrentUserIdAsync();
        var result = await _reportService.UpdateReportAsync(vm, userId);

        if (!result.IsSuccess)
        {
            var profileResult = await _reportService.GetUserProfileForAddingReportsAsync(userId);

            if (profileResult.IsSuccess)
            {
                vm.UserProfile = profileResult.Value.UserProfile;
            }
            ModelState.AddModelError("", result.ErrorMessage);
            return View(vm);
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = await GetCurrentUserIdAsync();
        var result = await _reportService.GetReportAsync(id, userId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return View("Error");
        }

        // Останется как страница подтверждения, если зайдут напрямую (не через модалку)
        return RedirectToAction("Index");
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken] // <-- важно
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var userId = await GetCurrentUserIdAsync();
        var result = await _reportService.DeleteReportAsync(id, userId);

        if (!result.IsSuccess)
            return View("Error", result.ErrorMessage);

        return RedirectToAction("Index");
    }


    [HttpGet]
    public async Task<IActionResult> Download(Guid id)
    {
        var userId = await GetCurrentUserIdAsync();
        var result = await _reportService.DownloadReportAsync(id, userId);

        if (!result.IsSuccess)
            return NotFound();

        return result.Value;
    }
}