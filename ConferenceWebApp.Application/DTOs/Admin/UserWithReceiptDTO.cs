namespace ConferenceWebApp.Application.DTOs.Admin;

public class UserWithReceiptDTO
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ReceiptFilePath { get; set; } = string.Empty;
    public bool HasPaid { get; set; }
}
