using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[Authorize]
public class PaymentController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IPaymentService _paymentService;
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        UserManager<User> userManager,
        IPaymentService paymentService,
        IUserProfileService userProfileService,
        ILogger<PaymentController> logger) : base(userProfileService)
    {
        _userManager = userManager;
        _paymentService = paymentService;
        _userProfileService = userProfileService;
        _logger = logger;
    }

    private async Task<(Guid? userId, IActionResult? redirect)> GetCurrentUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("Неавторизованный доступ к PaymentController");
            return (null, RedirectToAction("Login", "Auth"));
        }
        return (user.Id, null);
    }

    public async Task<IActionResult> Index()
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Загрузка страницы оплаты для UserId={UserId}", userId);

        var resultUserProfile = await _userProfileService.GetByUserIdAsync(userId!.Value);
        if (!resultUserProfile.IsSuccess)
        {
            _logger.LogError("Не удалось получить профиль пользователя {UserId}: {Error}",
                userId, resultUserProfile.ErrorMessage);
            ViewBag.Message = resultUserProfile.ErrorMessage;
            return View();
        }

        var resultReceipt = await _paymentService.GetReceiptByUserIdAsync(userId!.Value);
        if (!resultReceipt.IsSuccess)
        {
            _logger.LogWarning("У пользователя {UserId} нет чека: {Error}",
                userId, resultReceipt.ErrorMessage);

            return View(new ReceiptFileViewModel
            {
                UserProfile = resultUserProfile.Value,
                ReceiptFile = new ReceiptFileDTO()
            });
        }

        _logger.LogInformation("Чек найден для UserId={UserId}", userId);

        var vm = new ReceiptFileViewModel
        {
            UserProfile = resultUserProfile.Value,
            ReceiptFile = resultReceipt.Value
        };
        return View(vm);
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> UploadReceipt(ReceiptUploadDTO model)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        if (model?.Receipt == null || model.Receipt.Length == 0)
        {
            _logger.LogWarning("Попытка загрузки пустого файла. UserId={UserId}", userId);
            TempData["Error"] = "Файл чека не выбран.";
            return RedirectToAction("Index");
        }

        _logger.LogInformation("Загрузка чека. UserId={UserId}, File={File}, Size={Size}",
            userId, model.Receipt.FileName, model.Receipt.Length);

        var result = await _paymentService.UploadReceiptAsync(userId!.Value, model.Receipt);

        if (!result.IsSuccess)
        {
            _logger.LogError("Ошибка загрузки чека. UserId={UserId}: {Error}",
                userId, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage;
        }
        else
        {
            _logger.LogInformation("Чек успешно загружен. UserId={UserId}", userId);
            TempData["Success"] = "Чек успешно загружен";
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> DownloadReceipt()
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null) return redirect;

        _logger.LogInformation("Запрос на скачивание чека. UserId={UserId}", userId);

        var result = await _paymentService.DownloadReceiptAsync(userId!.Value);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Не удалось скачать чек. UserId={UserId}: {Error}",
                userId, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Index");
        }

        _logger.LogInformation("Чек отдан на скачивание. UserId={UserId}, FileName={FileName}",
            userId, result.Value.FileName);

        return File(result.Value.FileStream, result.Value.ContentType, result.Value.FileName);
    }
}
