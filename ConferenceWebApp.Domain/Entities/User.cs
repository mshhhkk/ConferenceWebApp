using Microsoft.AspNetCore.Identity;

namespace ConferenceWebApp.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public User() : base()
    {
    }
    public User(Guid id, string email) : base()
    {
        Id = id;
        Email = email;
        UserName = email;
        NormalizedEmail = email.ToUpperInvariant();
        NormalizedUserName = email.ToUpperInvariant();
        SecurityStamp = Guid.NewGuid().ToString();
    }
    public UserProfile UserProfile { get; set; } = null!;
}