using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.CommiteeDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

public class CommitteeController : BaseController
{
    private readonly ICommitteeService _committeeService;

    public CommitteeController(
        ICommitteeService committeeService,
        IUserProfileService userProfileService)
        : base(userProfileService)
    {
        _committeeService = committeeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _committeeService.GetAllAsync();

        if (!result.IsSuccess)
        {
            ViewBag.Message = result.ErrorMessage;
            return View(new List<CommitteeDTO>());
        }

        return View(result.Value);
    }
}