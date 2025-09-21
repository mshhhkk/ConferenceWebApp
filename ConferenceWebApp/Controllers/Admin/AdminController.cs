using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Application.Validation;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Infrastructure.Services.Realization;
using ConferenceWebApp.ViewModels.Admin;
using ConferenceWebApp.ViewModels.Admin;
using FluentValidation;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Composition;

namespace ConferenceWebApp.Application.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    private readonly IUserProfileService _userProfileService;
    private readonly UserManager<User> _userManager;
    private readonly IReportService _reportService;
    private readonly IPersonalAccountService _personalAccountService;
    public AdminController(IUserProfileService userProfileService, UserManager<User> userManager, IReportService reportService, IPersonalAccountService personalAccountService) : base(userProfileService)
    { 
        _userProfileService = userProfileService;
        _userManager = userManager;
        _reportService = reportService;
        _personalAccountService = personalAccountService;
    }

    public async Task<IActionResult> Index()
    {

        var resultUserProfiles = await _userProfileService.GetAllAsync();
        if (!resultUserProfiles.IsSuccess)
        {
            ViewBag.Message = resultUserProfiles.ErrorMessage;
            return View(new AdminUserProfileDTO());
        }
        return View(resultUserProfiles.Value);
    }

    [HttpGet]
    public IActionResult CreateUserWithReport()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserWithReport(CreateUserWithReportViewModel vm)
    {

        var userValidator = new EditUserProfileValidator();
        var userValidationResult = userValidator.Validate(vm.UserProfile);

        if (!userValidationResult.IsValid)
        {
            foreach (var failure in userValidationResult.Errors)
            {
                ModelState.AddModelError($"UserProfile.{failure.PropertyName}", failure.ErrorMessage);
            }
        }

        var reportValidator = new AddReportValidator();
        var reportValidationResult = reportValidator.Validate(vm.Report);

        if (!reportValidationResult.IsValid)
        {
            foreach (var failure in reportValidationResult.Errors)
            {
                ModelState.AddModelError($"Report.{failure.PropertyName}", failure.ErrorMessage);
            }
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = vm.Email, 
            UserName = vm.Email,
            NormalizedEmail = vm.Email.ToUpper(),
            NormalizedUserName = vm.Email.ToUpper(),
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Ошибка при создании пользователя.");
            return View(vm);
        }
            
        var profileResult = await _personalAccountService.UpdateProfileAsync(user.Id, vm.UserProfile);

        var reportResult = await _reportService.AddReportAsync(vm.Report, user.Id);

        ModelState.AddModelError("", "Ошибка при добавлении профиля или доклада.");
        
        return RedirectToAction("Index");
    }


    [HttpGet]
    public async Task<IActionResult> EditUser(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        var userProfile = await _userProfileService.AdminGetByUserIdAsync(id);

        if (userProfile == null)
        {
            return NotFound();
        }

        var vm = new AdminEditUserViewModel
        {
            Email = user.Email,
            UserProfile = userProfile.Value, 
                   
        };

        return View(vm);
   
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(Guid id, AdminEditUserViewModel vm)
    {

        var userValidator = new AdminEditUserValidator();
        var userValidationResult = userValidator.Validate(vm.UserProfile);

        if (!userValidationResult.IsValid)
        {
            foreach (var failure in userValidationResult.Errors)
            {
                ModelState.AddModelError($"UserProfile.{failure.PropertyName}", failure.ErrorMessage);
            }
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var userProfileUpdateResult = await _personalAccountService.AdminUpdateProfileAsync(id, vm.UserProfile);

        if (!userProfileUpdateResult.IsSuccess)
        {
            ModelState.AddModelError("", "Ошибка при обновлении профиля пользователя.");
            return View(vm);
        }

        return RedirectToAction("Index");
        
    }

    [HttpGet]
    public async Task<IActionResult> UserReports(Guid id) 
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        var reportsResult = await _reportService.GetReportsByUserIdAsync(id);
        if (!reportsResult.IsSuccess)
        {
            ViewBag.Message = reportsResult.ErrorMessage ?? reportsResult.ErrorMessage;
            return View(new AdminUserReportsViewModel
            {
                UserId = id,
                Email = user?.Email,
                Reports = new List<ReportDTO>()
            });
        }

        var vm = new AdminUserReportsViewModel
        {
            UserId = id,
            Email = user?.Email,
            Reports = reportsResult.Value ?? new List<ReportDTO>()
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult AddReport(Guid userId)
    {
        return View(new AddReportDTO());
    }

    [HttpPost]
    public async Task<IActionResult> AddReport(Guid userId, AddReportDTO dto)
    {
    
        var reportValidator = new AddReportValidator();
        var reportValidatorResult = reportValidator.Validate(dto);
        if (!reportValidatorResult.IsValid)
        {

            foreach (var failure in reportValidatorResult.Errors)
            {
                ModelState.AddModelError($"UserProfile.{failure.PropertyName}", failure.ErrorMessage);
            }

            return View(dto);
        }

        var res = await _reportService.AddReportAsync(dto, userId);


        TempData["Success"] = "Доклад добавлен.";
        return RedirectToAction(nameof(UserReports), new { id = userId });
    }

    [HttpGet]
    public async Task<IActionResult> EditReport(Guid userId, Guid reportId)
    {
        var reportResult = await _reportService.GetReportForEditAsync(reportId, userId);
        if (!reportResult.IsSuccess)
        {
            TempData["Error"] = reportResult.ErrorMessage;
            return RedirectToAction(nameof(UserReports), new { id = userId });
        }

        return View(reportResult.Value);
    }

    [HttpPost]
    public async Task<IActionResult> EditReport(Guid userId, EditReportDTO dto)
    {
        var reportValidator = new EditReportValidator();
        var reportValidatorResult = reportValidator.Validate(dto);
        if (!reportValidatorResult.IsValid)
        {

            foreach (var failure in reportValidatorResult.Errors)
            {
                ModelState.AddModelError($"UserProfile.{failure.PropertyName}", failure.ErrorMessage);
            }

            return View(dto);
        }

        var res = await _reportService.UpdateReportAsync(dto, userId);

        TempData["Success"] = "Доклад обновлён.";
        return RedirectToAction(nameof(UserReports), new { id = userId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteReport(Guid userId, Guid reportId)
    {
        var res = await _reportService.DeleteReportAsync(reportId, userId);
        TempData[res.IsSuccess ? "Success" : "Error"] = res.IsSuccess ? "Доклад удалён." : res.ErrorMessage;
        return RedirectToAction(nameof(UserReports), new { id = userId });
    }


    [HttpGet]
    public async Task<IActionResult> DownloadReport(Guid userId, Guid reportId)
    {
        var res = await _reportService.DownloadReportAsync(reportId, userId);
        if (!res.IsSuccess || res.Value == null)
        {
            TempData["Error"] = res.ErrorMessage;
            return RedirectToAction(nameof(UserReports), new { id = userId });
        }
        return res.Value;
    }

}
