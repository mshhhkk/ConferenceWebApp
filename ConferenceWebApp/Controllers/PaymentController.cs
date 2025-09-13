using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class PaymentController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IPaymentService _paymentService;
    private readonly IUserProfileRepository _userProfileRepository;

    public PaymentController(
        UserManager<User> userManager,
        IPaymentService paymentService,
        IUserProfileRepository userProfileRepository) : base(userProfileRepository)
    {
        _userManager = userManager;
        _paymentService = paymentService;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("Пользователь не найден.");

        var result = await _paymentService.GetReceiptAsync(user);
        if (!result.IsSuccess)
        {
            ViewBag.Message = result.ErrorMessage;
            return View("Error");
        }

        return View(result.Value);
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

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("Пользователь не найден.");

        var result = await _paymentService.UploadReceiptAsync(user, model.Receipt);
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
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("Пользователь не найден.");

        var result = await _paymentService.DownloadReceiptAsync(user);
        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }

        return File(result.Value.FileStream, result.Value.ContentType, result.Value.FileName);
    }
}