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
                .IsRequired(); // Указываем, что поле обязательно

                entity.Property(e => e.PhotoUrl)
                      .HasMaxLength(500)
                      .IsRequired();
            });

            builder.Entity<UserProfile>(entity =>
                {
                    entity.HasKey(up => up.UserId); // Указываем UserId как первичный ключ
                    entity.HasIndex(up => up.UserId)
                          .IsUnique(); // Уникальный индекс на UserId
                });

            builder.Entity<TwoFactorCode>(entity =>
            {
                entity.HasKey(e => e.Id); // Указываем первичный ключ

                entity.HasOne(e => e.User) // Связь с User
                      .WithMany() // Указываем, что User не содержит коллекции или навигационного свойства для TwoFactorCode
                      .HasForeignKey(e => e.UserId) // Внешний ключ
                      .IsRequired(); // Делаем связь обязательной

                entity.Property(e => e.Code)
                      .IsRequired()
                      .HasMaxLength(6); // Пример ограничения длины кода

                entity.Property(e => e.ExpirationTime)
                      .IsRequired(); // Указываем, что поле обязательно
            });

            //TODO ПЕРЕМЕСТИТЬ РОЛИ В ОДЕЛЬНЫЙ КЛАСС

            // Заранее подготовленные Guid для ролей (можно вынести в константы)
            var roles = new List<IdentityRole<Guid>>
            {
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("375FA642-7E6A-4333-B78F-270ED825997F"),
                    Name = "Participant",
                    NormalizedName = "PARTICIPANT"
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("6282CBD6-908D-4E0E-A3EE-6F41CD54FB5F"),
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("20AEDC71-9FC5-4272-B766-E6DC1AEB63AE"),
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN"
                }
            };

            // Добавляем роли в БД
            builder.Entity<IdentityRole<Guid>>().HasData(roles);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await base.SaveChangesAsync(ct);
        }
    }
}