using ConferenceWebApp.Application.Controllers;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Application.Interfaces.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminReportsController : BaseController
{
    private readonly IReportAdminService _reportService;
    private readonly IWebHostEnvironment _env;

    public AdminReportsController(
        IReportAdminService reportService,
        IUserProfileService userProfileService,
        IWebHostEnvironment env) : base(userProfileService)
    {
        _reportService = reportService;
        _env = env;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var model = await _reportService.GetFilteredReportsAsync(search);
        return View(model);
    }

    public async Task<IActionResult> Download(Guid id)
    {
        var relativePath = await _reportService.GetReportFilePathAsync(id);
        if (relativePath == null) return NotFound("Файл не найден.");

        var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
        if (!System.IO.File.Exists(fullPath)) return NotFound("Файл не найден.");

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        var fileName = Path.GetFileName(fullPath);
        return File(stream, "application/octet-stream", fileName);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(Guid id)
    {
        var report = await _reportService.GetReportByIdAsync(id);
        if (report == null) return NotFound();

        if (!Request.Form.ContainsKey("confirm"))
        {
            ViewBag.Report = report;
            return View("ConfirmApproval");
        }

        await _reportService.ApproveReportAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Rollback(Guid id)
    {
        var report = await _reportService.GetReportByIdAsync(id);
        if (report == null) return NotFound();

        if (!Request.Form.ContainsKey("confirm"))
        {
            ViewBag.Report = report;
            return View("ConfirmRollback");
        }

        await _reportService.RollbackReportAsync(id);
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Reject(Guid id)
    {
        var report = await _reportService.GetReportByIdAsync(id);
        if (report == null) return NotFound();

      
        return View(id);
    }


    [HttpPost]
    public async Task<IActionResult> Reject(Guid id, string comment)
    {
        var report = await _reportService.GetReportByIdAsync(id);
        if (report == null) return NotFound();

        await _reportService.RejectReportAsync(id, comment);

        TempData["Message"] = "Доклад отклонен успешно с комментариями.";
        return RedirectToAction(nameof(Index));
    }
}
