using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ConferenceWebApp.Domain.Enums;

namespace ConferenceWebApp.Domain.Entities;

public class Reports
{
    [Key]
    public Guid Id { get; set; } 

    [Required]
    public string ReportTheme { get; set; } = string.Empty;

    public string? ExtThesis { get; set; } 

    public string? ExtThesisFilePath { get; set; }

    [Required]
    public SectionTopic Section { get; set; } 

    [Required]
    public WorkType WorkType { get; set; } 

    [Required]
    public string FilePath { get; set; } = null!;

    [Required]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!; 

    [Required]
    public bool IsAuthor { get; set; }

    [Required]
    public Guid AuthorId { get; set; } = Guid.Empty;

    [Required]
    public Guid TargetUserId { get; set; } = Guid.Empty;

    [Required]
    public ReportStatus Status { get; set; }

    [Required]
    public ReportTransferStatus TransferStatus { get; set; }
}