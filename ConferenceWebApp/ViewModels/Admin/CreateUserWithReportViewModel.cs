using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using System;
using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Application.DTOs.Admin;

public class CreateUserWithReportViewModel
{
    [Required(ErrorMessage = "Email обязательно для заполнения.")]
    public string Email { get; set; }
    public AddReportDTO Report { get; set; }
    public EditUserDTO UserProfile { get; set; }
}
