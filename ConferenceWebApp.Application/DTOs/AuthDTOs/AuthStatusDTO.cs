namespace ConferenceWebApp.Application.DTOs.AuthDTOs;

public class AuthStatusDTO
{
    public bool IsAuthenticated { get; set; }
    public string? UserName { get; set; }
}