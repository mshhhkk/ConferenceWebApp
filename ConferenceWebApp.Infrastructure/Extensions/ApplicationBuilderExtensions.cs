using ConferenceWebApp.Domain.Constants;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task SeedIdentityAsync(this IServiceProvider services, bool migrate = false)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("SeedIdentity");
        var cfg = sp.GetRequiredService<IConfiguration>();

        if (migrate)
        {
            try
            {
                var db = sp.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();
                logger.LogInformation("Database.Migrate() выполнен.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка миграции БД");
            }
        }

        var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userMgr = sp.GetRequiredService<UserManager<User>>();

        var roles = new[] { SystemRoles.Participant, SystemRoles.Admin, SystemRoles.SuperAdmin };
        foreach (var role in roles)
            if (!await roleMgr.RoleExistsAsync(role))
                await roleMgr.CreateAsync(new IdentityRole<Guid> { Name = role, NormalizedName = role.ToUpperInvariant() });

        string? ReadSecret(string cfgKey, string envKey, string envFileKey)
        {
            var direct = cfg[cfgKey] ?? Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrWhiteSpace(direct)) return direct;

            var path = cfg[envFileKey] ?? Environment.GetEnvironmentVariable(envFileKey);
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                return File.ReadAllText(path).Trim();

            return null;
        }

        var email = cfg["SuperAdmin:Email"] ?? Environment.GetEnvironmentVariable("SUPERADMIN_EMAIL") ?? "superadmin@local";
        var password = ReadSecret("SuperAdmin:Password", "SUPERADMIN_PASSWORD", "SUPERADMIN_PASSWORD_FILE");
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("SUPERADMIN_PASSWORD не задан. Пропускаем создание супер-админа.");
            return;
        }

        var user = await userMgr.FindByEmailAsync(email);
        if (user == null)
        {
            user = new User { UserName = email, Email = email, EmailConfirmed = true };
            var create = await userMgr.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                logger.LogError("Не удалось создать SuperAdmin: {Errors}",
                    string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}")));
                return;
            }
        }

        if (!await userMgr.IsInRoleAsync(user, SystemRoles.SuperAdmin))
            await userMgr.AddToRoleAsync(user, SystemRoles.SuperAdmin);

        if (!await userMgr.IsInRoleAsync(user, SystemRoles.Admin))
            await userMgr.AddToRoleAsync(user, SystemRoles.Admin);
    }
}
