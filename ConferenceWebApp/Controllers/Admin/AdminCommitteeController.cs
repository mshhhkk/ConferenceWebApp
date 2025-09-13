using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Infrastructure.Services.Abstract.Admin;
using Microsoft.AspNetCore.Mvc;

public class AdminCommitteeController : BaseController
{
    private readonly IAdminCommitteeService _committeeService;

    public AdminCommitteeController(
        IAdminCommitteeService committeeService,
        IUserProfileRepository userProfileRepository) : base(userProfileRepository)
    {
        _committeeService = committeeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var committees = await _committeeService.GetAllCommitteesAsync();
        return View(committees);
    }

    [HttpGet]
    public IActionResult Add() => View();

    [HttpPost]
    public async Task<IActionResult> Add(AdminCommitteeDTO dto, IFormFile? Photo)
    {
        if (!ModelState.IsValid) return View(dto);

        await _committeeService.AddCommitteeAsync(dto, Photo);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _committeeService.GetCommitteeByIdAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(AdminCommitteeDTO dto, IFormFile? Photo)
    {
        if (!ModelState.IsValid) return View(dto);

        var success = await _committeeService.EditCommitteeAsync(dto, Photo);
        if (!success) return NotFound();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _committeeService.DeleteCommitteeAsync(id);
        return RedirectToAction("Index");
    }
}