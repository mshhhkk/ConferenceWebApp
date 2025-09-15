using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ConferenceWebApp.Application.ViewModels;

[Authorize]
public class PaymentController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IPaymentService _paymentService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserProfileService _userProfileService;

    public PaymentController(
        UserManager<User> userManager,
        IPaymentService paymentService,
        IUserProfileRepository userProfileRepository, IUserProfileService userProfileService) : base(userProfileRepository)
    {
        _userManager = userManager;
        _paymentService = paymentService;
        _userProfileRepository = userProfileRepository;
        _userProfileService = userProfileService;
    }

    private async Task<(Guid? userId, IActionResult? redirect)> GetCurrentUserIdAsync()
    {

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return (null, RedirectToAction("Login", "Account"));
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


    [HttpPost]
    public async Task<IActionResult> UploadReceipt(ReceiptUploadDTO model)
    {
        if (!ModelState.IsValid)
        {
            TempData["UploadError"] = ModelState.Values
                .SelectMany(v => v.Errors)
                .FirstOrDefault()?.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        var (userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var result = await _paymentService.UploadReceiptAsync(userId!.Value, model.Receipt);

        if (!result.IsSuccess)
        {
            TempData["UploadError"] = result.ErrorMessage;
        }
        else
        {
            TempData["Message"] = "Чек успешно загружен";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DownloadReceipt()
    {
        var(userId, redirect) = await GetCurrentUserIdAsync();
        if (redirect != null)
            return redirect;

        var result = await _paymentService.DownloadReceiptAsync(userId!.Value);
        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }

        return File(result.Value.FileStream, result.Value.ContentType, result.Value.FileName);
    }
}