using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ConferenceWebApp.Application.Controllers;

public class BaseController : Controller
{
    private readonly IUserProfileRepository _userProfileRepository;

    public BaseController(IUserProfileRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }

    protected void AddError(string errorMessage)
    {
        TempData["Error"] = errorMessage;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        bool isAuthenticated = User.Identity?.IsAuthenticated ?? false;
        bool isRegisteredOnConference = false;
        string username = string.Empty;

        if (isAuthenticated)
        {
            var email = User.Identity?.Name;
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userProfileRepository.GetUserProfileByEmail(email);
                if (user != null)
                {
                    isRegisteredOnConference = await _userProfileRepository.IsUserRegisteredForConferenceAsync(user.UserId);
                    username = $"{user.LastName} {user.FirstName?[0]}";
                }
            }
        }

        var authStatus = new AuthStatusDTO
        {
            IsAuthenticated = isAuthenticated,
            IsRegisteredOnConference = isRegisteredOnConference,
            UserName = username
        };

        ViewBag.AuthStatus = authStatus;

        if (TempData["Error"] is string error)
        {
            ViewBag.Error = error;
            TempData.Remove("Error");
        }
        await next(); // Продолжаем выполнение следующего действия
    }
}