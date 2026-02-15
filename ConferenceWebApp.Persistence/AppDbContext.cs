using ConferenceWebApp.Domain.Constants;
using ConferenceWebApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConferenceWebApp.Persistence
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        public DbSet<Reports> Reports { get; set; } = null!;
        public DbSet<UserProfile> UserProfile { get; set; } = null!;
        public DbSet<Committee> Committee { get; set; } = null!;
        public DbSet<Schedule> Schedule { get; set; } = null!;
        public DbSet<TwoFactorCode> TwoFactorCode { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            builder.Entity<Reports>(entity =>
            {
                entity.Property(e => e.Section)
                      .HasConversion<string>();

                entity.Property(e => e.WorkType)
                      .HasConversion<string>();
            });

            builder.Entity<User>(entity =>
            {
                entity.HasOne(u => u.UserProfile)
                .WithOne(up => up.User)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .IsRequired();
            });
            builder.Entity<Schedule>(entity =>
            {
                entity.Property(e => e.Description)
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(e => e.StartingTime)
                      .HasConversion<string>();

                entity.Property(e => e.EndingTime)
                      .HasConversion<string>();
            });


            builder.Entity<Committee>(entity =>
            {
                entity.Property(e => e.FullName)
                      .HasMaxLength(255)
                      .IsRequired();

                entity.Property(e => e.Description)
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(e => e.IsHead)
                .IsRequired();

                entity.Property(e => e.PhotoUrl)
                      .HasMaxLength(500)
                      .IsRequired();
            });

            builder.Entity<UserProfile>(entity =>
                {
                    entity.HasKey(up => up.UserId);
                    entity.HasIndex(up => up.UserId)
                          .IsUnique();
                });

            builder.Entity<TwoFactorCode>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .IsRequired();

                entity.Property(e => e.Code)
                      .IsRequired()
                      .HasMaxLength(6);

                entity.Property(e => e.ExpirationTime)
                      .IsRequired();
            });

            builder.Entity<IdentityRole<Guid>>().HasData(

                new IdentityRole<Guid>
                {
                    Id = SystemRoles.ParticipantId,
                    Name = SystemRoles.Participant,
                    NormalizedName = SystemRoles.Participant.ToUpperInvariant()
                },
                new IdentityRole<Guid>
                {
                    Id = SystemRoles.AdminId,
                    Name = SystemRoles.Admin,
                    NormalizedName = SystemRoles.Admin.ToUpperInvariant()
                },
                new IdentityRole<Guid>
                {
                    Id = SystemRoles.SuperAdminId,
                    Name = SystemRoles.SuperAdmin,
                    NormalizedName = SystemRoles.SuperAdmin.ToUpperInvariant()
                });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await base.SaveChangesAsync(ct);
        }
    }
}