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
    private readonly ILogger<ExtendedThesisController> _logger; // 👈 добавляем логгер

    public ExtendedThesisController(
        UserManager<User> userManager,
        IExtendedThesisService thesisService,
        IUserProfileService userProfileService,
        ILogger<ExtendedThesisController> logger) // 👈 внедряем
        : base(userProfileService)
    {
        _userManager = userManager;
        _thesisService = thesisService;
        _userProfileService = userProfileService;
        _logger = logger;
    }

    private async Task<(Guid? userId, IActionResult? redirect)> GetCurrentUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("Неавторизованный доступ к {Action}", nameof(ExtendedThesisController));
            return (null, RedirectToAction("Login", "Auth"));
        }
        return (user.Id, null);
    }

    public async Task<IActionResult> Index()
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Загрузка списка расширенных тезисов для UserId={UserId}", userId);

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId!.Value);
        if (!resultUserProfile.IsSuccess)
        {
            _logger.LogError("Не удалось получить профиль пользователя {UserId}: {Error}", userId, resultUserProfile.ErrorMessage);
            ViewBag.Message = resultUserProfile.ErrorMessage;
            return View(new ExtendedThesisViewModel());
        }

        var resultExtThesis = await _thesisService.GetExtendedThesisesAsync(userId!.Value);
        if (!resultExtThesis.IsSuccess)
        {
            _logger.LogError("Ошибка загрузки тезисов для {UserId}: {Error}", userId, resultExtThesis.ErrorMessage);
            ViewBag.ErrorMessage = resultExtThesis.ErrorMessage;
            return View(new ExtendedThesisViewModel());
        }

        _logger.LogInformation("Пользователь {UserId} загрузил {Count} тезисов", userId, resultExtThesis.Value.ReportsWithTheses.Count);

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
        if (redirect != null) return redirect;

        _logger.LogInformation("Пользователь {UserId} открыл редактирование тезиса ReportId={ReportId}", userId, reportId);

        var result = await _thesisService.GetThesisAsync(userId!.Value, reportId);
        if (!result.IsSuccess)
        {
            _logger.LogError("Ошибка получения тезиса ReportId={ReportId}, UserId={UserId}: {Error}", reportId, userId, result.ErrorMessage);
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
        if (redirect != null) return redirect;

        _logger.LogInformation("Пользователь {UserId} сохраняет изменения тезиса ReportId={ReportId}", userId, vm.Thesis.ReportId);

        var resultEdit = await _thesisService.GetThesisAsync(userId!.Value, vm.Thesis.ReportId);
        if (!resultEdit.IsSuccess)
        {
            _logger.LogError("Ошибка загрузки тезиса для редактирования ReportId={ReportId}: {Error}", vm.Thesis.ReportId, resultEdit.ErrorMessage);
            ViewBag.ErrorMessage = resultEdit.ErrorMessage;
            return View(vm);
        }

        var result = await _thesisService.UpdateExtendedThesisAsync(userId!.Value, vm.Thesis);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Не удалось обновить тезис ReportId={ReportId}, UserId={UserId}: {Error}", vm.Thesis.ReportId, userId, result.ErrorMessage);
            ViewBag.ErrorMessage = result.ErrorMessage;
            return View(vm);
        }

        _logger.LogInformation("Пользователь {UserId} успешно обновил тезис ReportId={ReportId}", userId, vm.Thesis.ReportId);
        return RedirectToAction(nameof(Index));
    }
}
