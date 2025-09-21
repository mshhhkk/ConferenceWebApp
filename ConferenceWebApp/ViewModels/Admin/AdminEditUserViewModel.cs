using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.ViewModels.Admin;

public class AdminEditUserViewModel
{
    [Required(ErrorMessage = "Email обязательно для заполнения.")]
    public string Email { get; set; }
    public AdminEditUserDTO UserProfile { get; set; }
}
