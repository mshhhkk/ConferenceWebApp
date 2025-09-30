using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ConferenceWebApp.Domain.Enums;

namespace ConferenceWebApp.Domain.Entities;

public class UserProfile
{
    [Key]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    public User User { get; set; } = null;

    public string FirstName { get; set; } = "Имя";

    public string LastName { get; set; } = "Фамилия";

    public string? MiddleName { get; set; }

    public DateOnly BirthDate { get; set; } 

    public string? Organization { get; set; } = string.Empty;

    public string? Specialization { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; } = "/images/defaultUserPhoto.png";

    [Required]
    public ParticipantType ParticipantType { get; set; } = ParticipantType.Spectator;

    [Required]
    public ParticipantStatus Status { get; set; }

    public UserApprovalStatus ApprovalStatus { get; set; }

    [MaxLength(255)]
    public string? ReceiptFilePath { get; set; }

    [Required]
    public Position Position { get; set; }

    [Required]
    public ScientificDegree Degree { get; set; }
}