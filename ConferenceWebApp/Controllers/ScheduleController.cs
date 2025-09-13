using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

public class ScheduleController : BaseController
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(
        IUserProfileRepository userProfileRepository,
        IScheduleService scheduleService)
        : base(userProfileRepository)
    {
        _scheduleService = scheduleService;
    }

    public async Task<IActionResult> Index()
    {
        var groupedSchedules = await _scheduleService.GetAllGroupedSchedules();
        return View(groupedSchedules);
    }
}