using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.CommiteeDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public class CommitteeController : BaseController
{
    private readonly ICommitteeService _committeeService;
    private readonly ILogger<CommitteeController> _logger;

    public CommitteeController(
        ICommitteeService committeeService,
        IUserProfileService userProfileService,
        ILogger<CommitteeController> logger)
        : base(userProfileService)
    {
        _committeeService = committeeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            _logger.LogInformation("Запрошен список комитета");

            var result = await _committeeService.GetAllAsync();

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Не удалось получить список комитета: {Error}", result.ErrorMessage);
                ViewBag.Message = result.ErrorMessage;
                return View(new List<CommitteeDTO>());
            }

            _logger.LogInformation("Получено членов комитета: {Count}", result.Value?.Count ?? 0);
            return View(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка комитета");
            ViewBag.Message = "Не удалось загрузить состав комитета. Попробуйте позже.";
            return View(new List<CommitteeDTO>());
        }
    }
}
