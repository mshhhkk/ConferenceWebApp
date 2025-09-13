using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.DTOs.CommiteeDTOs;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceWebApp.Application.Controllers;

public class HomeController : BaseController
{
    private readonly ICommitteeService _committeeService;
    public HomeController(IUserProfileRepository userProfileRepository, ICommitteeService committeeService) : base(userProfileRepository)
    {
        _committeeService = committeeService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _committeeService.GetAllCommitteesAsync();

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(new List<CommitteeDTO>());
        }
        return View(result.Value);
    }
    public IActionResult Error()
    {
        ViewBag.ErrorMessage = TempData["ErrorMessage"] ?? "Произошла ошибка";
        return View();
    }
}