using ConferenceWebApp.Domain.Enums;

namespace ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

public class UserProfileDTO
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string Organization { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    private string? _photoUrl;

    public string PhotoUrl { get; set; } = "/images/user.svg";

    public ParticipantType ParticipantType { get; set; }
    public ParticipantStatus Status { get; set; }
    public UserApprovalStatus ApprovalStatus { get; set; }
    public bool IsApprovedAnyReports { get; set; }
    public bool IsExtendedThesisApproved { get; set; }

    public void SetPhotoUrl(string? photoUrl)
    {
        if (string.IsNullOrEmpty(photoUrl))
        {
            PhotoUrl = "/images/user.svg";  // Если URL пустой, ставим дефолтный
        }
        else
        {
            PhotoUrl = photoUrl;
        }
    }
}