using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using ConferenceWebApp.Models;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class ErrorController : Controller
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(IWebHostEnvironment env, ILogger<ErrorController> logger)
    {
        _env = env; _logger = logger;
    }

    [Route("error")]
    public IActionResult Error()
    {
        var vm = new ErrorViewModel
        {
            Title = "Упс, что-то пошло не так",
            Message = "Попробуйте позже. Если ошибка повторяется — сообщите нам."
        };

        Response.StatusCode = 500; // важно, чтобы был реальный статус
        return View("FriendlyError", vm);
    }




    [Route("error/{code:int}")]
    public IActionResult Code(int code)
    {
        var vm = new ErrorViewModel
        {
            Title = code switch
            {
                404 => "Страница не найдена",
                403 => "Доступ запрещён",
                401 => "Требуется авторизация",
                _ => "Произошла ошибка"
            },
            Message = code switch
            {
                404 => "Похоже, такой страницы нет или она переехала.",
                403 => "У вас нет прав для просмотра этой страницы.",
                401 => "Пожалуйста, войдите и попробуйте снова.",
                _ => "Неожиданная ошибка. Попробуйте позже."
            },
        };

        Response.StatusCode = code;
        return View("FriendlyError", vm);
    }
}
