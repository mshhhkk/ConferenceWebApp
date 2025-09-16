using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Application.DTOs.AuthDTOs;

public class Verify2SADTO
{
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Код обязателен")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Код должен содержать 6 символов")]
    public string Code { get; set; } = string.Empty;
}