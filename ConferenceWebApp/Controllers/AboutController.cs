using Microsoft.AspNetCore.Mvc;
using ConferenceWebApp.Application.Interfaces.Services;

namespace ConferenceWebApp.Application.Controllers;

public class AboutController : BaseController
{

    public AboutController(IUserProfileService userProfileService) : base(userProfileService) { }

    public IActionResult Index()
    {
        return View();
    }

}