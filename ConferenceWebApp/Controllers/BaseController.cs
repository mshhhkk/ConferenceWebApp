using ConferenceWebApp.Application.DTOs.AuthDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace ConferenceWebApp.Application.Controllers;

public class BaseController : Controller
{
    private readonly IUserProfileService _userProfileService;

    public BaseController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        bool isAuthenticated = User.Identity?.IsAuthenticated ?? false;
        string username = string.Empty;
        if (isAuthenticated)
        {
            var email = User.Identity?.Name;

            if (!string.IsNullOrEmpty(email))
            {
                username = await _userProfileService.GetUserNameByEmailAsync(email);
            }
        }

        var authStatus = new AuthStatusDTO
        {
            IsAuthenticated = isAuthenticated,
            UserName = username
        };

        ViewBag.AuthStatus = authStatus;

        await next();
    }
}