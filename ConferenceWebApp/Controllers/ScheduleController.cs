using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public class ScheduleController : BaseController
{
    private readonly IScheduleService _scheduleService;
    private readonly ILogger<ScheduleController> _logger;

    public ScheduleController(
        IUserProfileService userProfileService,
        IScheduleService scheduleService,
        ILogger<ScheduleController> logger)
        : base(userProfileService)
    {
        _scheduleService = scheduleService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            _logger.LogInformation("Запрос на отображение расписания");

            var groupedSchedules = await _scheduleService.GetAllGroupedSchedules();

            if (groupedSchedules == null || !groupedSchedules.Any())
            {
                _logger.LogWarning("Сервис вернул пустое расписание");
            }
            else
            {
                _logger.LogInformation("Расписание успешно получено. Кол-во групп: {Count}", groupedSchedules.Count());
            }

            return View(groupedSchedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении расписания");
            TempData["Error"] = "Не удалось загрузить расписание, попробуйте позже.";
            return View("Error"); // можно сделать отдельную вьюху с сообщением
        }
    }
}
