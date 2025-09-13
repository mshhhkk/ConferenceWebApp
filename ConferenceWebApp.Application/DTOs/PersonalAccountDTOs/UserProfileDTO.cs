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

    public string PhotoUrl
    {
        get => string.IsNullOrEmpty(_photoUrl) ? "/images/defaultUserPhoto.png" : _photoUrl;
        set => _photoUrl = value;
    }

    public ParticipantType ParticipantType { get; set; }
    public bool HasPaidFee { get; set; }
    public bool IsRegisteredForConference { get; set; }
    public bool IsApprovedAnyReports { get; set; }
}