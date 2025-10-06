using ConferenceWebApp.Application.DTOs.Admin;
using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.ViewModels.Admin;

public class AdminEditUserViewModel
{
    [Required(ErrorMessage = "Email обязательно для заполнения.")]
    public string Email { get; set; } = string.Empty;
    public AdminEditUserDTO UserProfile { get; set; } = new AdminEditUserDTO();
}
