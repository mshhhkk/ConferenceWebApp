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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

[Authorize]
public class ReportsController : BaseController
{
    private readonly IReportService _reportService;
    private readonly UserManager<User> _userManager;
    private readonly IUserProfileService _userProfileService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportService reportService,
        UserManager<User> userManager,
        IUserProfileService userProfileService,
        ISessionService sessionService,
        ILogger<ReportsController> logger) : base(userProfileService)
    {
        _reportService = reportService;
        _userManager = userManager;
        _userProfileService = userProfileService;
        _sessionService = sessionService;
        _logger = logger;
    }

    private async Task<(Guid? userId, IActionResult? redirect)> GetCurrentUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("Неавторизованный доступ к Reports.*");
            return (null, RedirectToAction("Login", "Auth"));
        }
        return (user.Id, null);
    }

    public async Task<IActionResult> Index()
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Открыт список докладов. UserId={UserId}", userId);

        var userProfileJson = HttpContext.Session.GetString("UserProfile");
        if (string.IsNullOrEmpty(userProfileJson))
        {
            _logger.LogWarning("Сессия без UserProfile. Redirect -> Login. UserId={UserId}", userId);
            return RedirectToAction("Login", "Auth");
        }

        var userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJson);
        if (userProfile == null)
        {
            _logger.LogError("Не удалось десериализовать UserProfile из сессии. UserId={UserId}", userId);
            TempData["Error"] = "Не удалось загрузить профиль пользователя.";
            return RedirectToAction("Login", "Auth");
        }

        var reportsResult = await _reportService.GetReportsByUserIdAsync(userId!.Value);
        if (!reportsResult.IsSuccess)
        {
            _logger.LogWarning("Ошибка получения списка докладов. UserId={UserId}: {Error}",
                userId, reportsResult.ErrorMessage);

            return View(new UserReportsViewModel
            {
                UserProfile = userProfile,
                Reports = new List<ReportDTO>()
            });
        }

        _logger.LogInformation("Загружено {Count} доклад(ов). UserId={UserId}",
            reportsResult.Value.Count, userId);

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
        if (redirect != null) return redirect;

        var userProfileJson = HttpContext.Session.GetString("UserProfile");
        if (string.IsNullOrEmpty(userProfileJson))
        {
            _logger.LogWarning("Add(): нет UserProfile в сессии. Redirect -> Login. UserId={UserId}", userId);
            return RedirectToAction("Login", "Auth");
        }

        var userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJson);
        _logger.LogInformation("Открыта форма добавления доклада. UserId={UserId}", userId);

        return View(new AddReportViewModel { UserProfile = userProfile, Report = new AddReportDTO() });
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Add(AddReportViewModel vm)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        var userProfileJson = HttpContext.Session.GetString("UserProfile");
        if (string.IsNullOrEmpty(userProfileJson))
        {
            _logger.LogWarning("POST Add(): нет UserProfile в сессии. Redirect -> Login. UserId={UserId}", userId);
            return RedirectToAction("Login", "Auth");
        }

        var userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJson);

        var validator = new AddReportValidator();
        var validationResult = validator.Validate(vm.Report);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Валидация не пройдена при добавлении доклада. UserId={UserId}; Ошибки={Errors}",
                userId, string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
            vm.UserProfile = userProfile;
            return View(vm);
        }

        _logger.LogInformation("Добавление доклада. UserId={UserId}, Theme={Theme}, Section={Section}, WorkType={WorkType}",
            userId, vm.Report?.ReportTheme, vm.Report?.Section, vm.Report?.WorkType);

        var result = await _reportService.AddReportAsync(vm.Report, userId!.Value);
        if (!result.IsSuccess)
        {
            _logger.LogError("Ошибка при добавлении доклада. UserId={UserId}: {Error}",
                userId, result.ErrorMessage);
            ViewBag.Message = result.ErrorMessage;
            vm.UserProfile = userProfile;
            return View(vm);
        }

        _logger.LogInformation("Доклад успешно добавлен. UserId={UserId}", userId);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        var userProfileJson = HttpContext.Session.GetString("UserProfile");
        if (string.IsNullOrEmpty(userProfileJson))
        {
            _logger.LogWarning("Edit(id) GET: нет UserProfile в сессии. Redirect -> Login. UserId={UserId}", userId);
            return RedirectToAction("Login", "Auth");
        }

        var userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJson);

        _logger.LogInformation("Открыто редактирование доклада. UserId={UserId}, ReportId={ReportId}", userId, id);

        var result = await _reportService.GetReportForEditAsync(id, userId!.Value);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Не удалось получить доклад для редактирования. UserId={UserId}, ReportId={ReportId}: {Error}",
                userId, id, result.ErrorMessage);
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
        if (redirect != null) return redirect;

        var userProfileJson = HttpContext.Session.GetString("UserProfile");
        if (string.IsNullOrEmpty(userProfileJson))
        {
            _logger.LogWarning("POST Edit: нет UserProfile в сессии. Redirect -> Login. UserId={UserId}", userId);
            return RedirectToAction("Login", "Auth");
        }

        var validator = new EditReportValidator();
        var validationResult = validator.Validate(vm.Report);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Валидация не пройдена при редактировании доклада. UserId={UserId}; Ошибки={Errors}",
                userId, string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJson);
            vm.UserProfile = userProfile;
            return View(vm);
        }

        _logger.LogInformation("Сохранение изменений доклада. UserId={UserId}, ReportId={ReportId}",
            userId, vm.Report?.Id);

        var result = await _reportService.UpdateReportAsync(vm.Report, userId!.Value);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Не удалось обновить доклад. UserId={UserId}, ReportId={ReportId}: {Error}",
                userId, vm.Report?.Id, result.ErrorMessage);
            ViewBag.Message = result.ErrorMessage;

            var userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJson);
            vm.UserProfile = userProfile;
            return View(vm);
        }

        _logger.LogInformation("Доклад обновлён. UserId={UserId}, ReportId={ReportId}", userId, vm.Report?.Id);
        return RedirectToAction("Index");
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Удаление доклада. UserId={UserId}, ReportId={ReportId}", userId, id);

        var result = await _reportService.DeleteReportAsync(id, userId!.Value);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Не удалось удалить доклад. UserId={UserId}, ReportId={ReportId}: {Error}",
                userId, id, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Index");
        }

        _logger.LogInformation("Доклад удалён. UserId={UserId}, ReportId={ReportId}", userId, id);
        TempData["Success"] = "Доклад успешно удален!";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Download(Guid id)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Скачивание доклада. UserId={UserId}, ReportId={ReportId}", userId, id);

        var result = await _reportService.DownloadReportAsync(id, userId!.Value);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Не удалось скачать доклад. UserId={UserId}, ReportId={ReportId}: {Error}",
                userId, id, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Index");
        }

        _logger.LogInformation("Доклад отдан на скачивание. UserId={UserId}, ReportId={ReportId}", userId, id);
        return result.Value; // FileResult
    }
}
