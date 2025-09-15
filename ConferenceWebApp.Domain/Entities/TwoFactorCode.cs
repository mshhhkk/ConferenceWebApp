using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConferenceWebApp.Domain.Entities;

public class TwoFactorCode
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public DateTime ExpirationTime { get; set; }
}