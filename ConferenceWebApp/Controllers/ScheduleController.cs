using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

public class ScheduleController : BaseController
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(
        IUserProfileService userProfileService,
        IScheduleService scheduleService)
        : base(userProfileService)
    {
        _scheduleService = scheduleService;
    }

    public async Task<IActionResult> Index()
    {
        var groupedSchedules = await _scheduleService.GetAllGroupedSchedules();
        return View(groupedSchedules);
    }
}