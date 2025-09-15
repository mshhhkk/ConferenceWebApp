using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Application.Interfaces.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminScheduleController : BaseController
{
    private readonly IScheduleAdminService _scheduleService;

    public AdminScheduleController(IScheduleAdminService scheduleService, IUserProfileService userProfileService)
        : base(userProfileService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var schedules = await _scheduleService.GetGroupedScheduleAsync();
        return View(schedules);
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(AdminScheduleDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _scheduleService.AddScheduleAsync(dto);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _scheduleService.GetScheduleForEditAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminScheduleDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var success = await _scheduleService.UpdateScheduleAsync(dto);
        if (!success)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _scheduleService.DeleteScheduleAsync(id);
        return RedirectToAction(nameof(Index));
    }
}