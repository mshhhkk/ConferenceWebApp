using ConferenceWebApp.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceWebApp.Application.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        public AdminController(IUserProfileRepository userProfileRepository) : base(userProfileRepository)
        { }

        // GET: AdminController
        public ActionResult Index()
        {
            return View();
        }
    }
}