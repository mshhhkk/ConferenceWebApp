using ConferenceWebApp.Application.DTOs.CommiteeDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Application.Controllers;

public class HomeController : BaseController
{
    private readonly ICommitteeService _committeeService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IUserProfileService userProfileService,
        ICommitteeService committeeService,
        ILogger<HomeController> logger)
        : base(userProfileService)
    {
        _committeeService = committeeService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            _logger.LogInformation("Запрошена главная страница (Home/Index)");

            ViewBag.Message = TempData["Error"];

            var result = await _committeeService.GetAllAsync();
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Ошибка при получении списка комитета: {Error}", result.ErrorMessage);
                TempData["Error"] = result.ErrorMessage;
                return View(new List<CommitteeDTO>());
            }

            _logger.LogInformation("Главная страница загружена. Количество членов комитета: {Count}", result.Value.Count);
            return View(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критическая ошибка при загрузке главной страницы");
            TempData["Error"] = "Не удалось загрузить данные. Попробуйте позже.";
            return View(new List<CommitteeDTO>());
        }
    }
}
