namespace ConferenceWebApp.Application.DTOs.AuthDTOs;

public class AuthStatusDTO
{
    public required bool IsAuthenticated { get; set; }
    public required string UserName { get; set; } = string.Empty;
}