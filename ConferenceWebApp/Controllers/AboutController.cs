using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceWebApp.Application.Controllers;

public class AboutController : BaseController
{

    public AboutController(IUserProfileService userProfileService) : base(userProfileService) { }

    public IActionResult Index()
    {
        return View();
    }

}