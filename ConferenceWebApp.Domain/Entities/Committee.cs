using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Domain.Entities;

public class Committee
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string PhotoUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public bool IsHead { get; set; } = false;
}