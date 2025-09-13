using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.DTOs.CommiteeDTOs;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

public class CommitteeController : BaseController
{
    private readonly ICommitteeService _committeeService;

    public CommitteeController(
        ICommitteeService committeeService,
        IUserProfileRepository userProfileRepository)
        : base(userProfileRepository)
    {
        _committeeService = committeeService;
    }

    [HttpGet]
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
}