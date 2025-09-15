using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class PaymentController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IPaymentService _paymentService;
    private readonly IUserProfileService _userProfileService;

    public PaymentController(
        UserManager<User> userManager,
        IPaymentService paymentService,
        IUserProfileService userProfileService) : base(userProfileService)
    {
        _userManager = userManager;
        _paymentService = paymentService;
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
            return View();
        }

        var resultReceipt = await _paymentService.GetReceiptByUserIdAsync(userId!.Value);
        if (!resultReceipt.IsSuccess)
        {
            ViewBag.Message = resultReceipt.ErrorMessage;
            return View(new ReceiptFileViewModel { UserProfile = resultUserProfile.Value, ReceiptFile = new ReceiptFileDTO() });
        }

        var vm = new ReceiptFileViewModel
        {
            UserProfile = resultUserProfile.Value,
            ReceiptFile = resultReceipt.Value,
        };
        return View(vm);
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> UploadReceipt(ReceiptUploadDTO model)
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var result = await _paymentService.UploadReceiptAsync(userId!.Value, model.Receipt);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
        }
        else
        {
            TempData["Success"] = "Чек успешно загружен";
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> DownloadReceipt()
    {
        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var result = await _paymentService.DownloadReceiptAsync(userId!.Value);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Index");
        }

        return File(result.Value.FileStream, result.Value.ContentType, result.Value.FileName);
    }
}