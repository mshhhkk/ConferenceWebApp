using ConferenceWebApp.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConferenceWebApp.Domain.Entities;

public class UserProfile
{
    [Key]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    public User User { get; set; } = default!; //means NOTNULL!

    private string _firstname = "Имя";

    public string FirstName {
        get => _firstname;
        set => _firstname = string.IsNullOrWhiteSpace(value) ? "Имя" : value;
        }


    private string _lastname = "Фамилия";

    public string LastName
    {
        get => _lastname;
        set => _lastname = string.IsNullOrWhiteSpace(value) ? "Фамилия" : value;
    }


    public string? MiddleName { get; set; }

    public DateOnly? BirthDate { get; set; }

    private string _organization = string.Empty;
    public string? Organization
    {
        get => _organization;
        set => _organization = value ?? string.Empty;
    }
        
    private string _specialization = string.Empty;
    public string? Specialization
    {
        get => _specialization;
        set =>  _specialization = value ?? string.Empty;
    }

    public string? PhoneNumber { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; } = "/images/user.svg";

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