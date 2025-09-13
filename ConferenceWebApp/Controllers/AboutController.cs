using ConferenceWebApp.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceWebApp.Application.Controllers;

public class AboutController : BaseController
{

    public AboutController(IUserProfileRepository userProfileRepository) : base(userProfileRepository) { }

    public async Task<IActionResult> Index()
    {

        return View();
    }

}