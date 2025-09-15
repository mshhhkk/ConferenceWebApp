using ConferenceWebApp.Application.DTOs.CommiteeDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceWebApp.Application.Controllers;

public class HomeController : BaseController
{
    private readonly ICommitteeService _committeeService;
    public HomeController(IUserProfileService userProfileService, ICommitteeService committeeService) : base(userProfileService)
    {
        _committeeService = committeeService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Message = TempData["Error"];
        var result = await _committeeService.GetAllAsync();

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(new List<CommitteeDTO>());
        }

        return View(result.Value);
    }
}
