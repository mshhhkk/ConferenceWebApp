using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceWebApp.Application.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        public AdminController(IUserProfileService userProfileService) : base(userProfileService)
        { }

        // GET: AdminController
        public ActionResult Index()
        {
            return View();
        }
    }
}