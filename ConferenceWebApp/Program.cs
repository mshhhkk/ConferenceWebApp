using ConferenceWebApp.Application.Extensions;
using ConferenceWebApp.Domain.Constants;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Infrastructure.Extensions;
using ConferenceWebApp.Middleware;
using ConferenceWebApp.Persistence;
using ConferenceWebApp.Persistence.Extensions;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace ConferenceWebApp;

public class Program
{
    public static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(builder.Environment.ContentRootPath, "Logs", "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true
            )
            .CreateLogger();

        builder.Host.UseSerilog();

        try
        {
            Log.Information("Приложение запускается...");

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDistributedMemoryCache();


            builder.Services.AddDatabase(builder.Configuration);
            builder.Services.AddApplicationServices();
            builder.Services.AddPersistence();


            builder.Services.AddValidators();

            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            builder.Services.AddInfrastructure(rootPath);

            builder.Services.AddScoped<UserManager<User>, CustomUserManager>();
            builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.ProviderMap.Add("Email", new TokenProviderDescriptor(typeof(EmailTokenProvider<User>)));
                options.Tokens.EmailConfirmationTokenProvider = "Email";
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Auth/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(20);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.None
                    : CookieSecurePolicy.Always;
            });

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseExceptionHandler("/error");
                app.UseStatusCodePagesWithReExecute("/error/{0}");
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseMiddleware<SessionMiddleware>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                async Task EnsureRoleAsync(Guid id, string name)
                {
                    if (!await roleManager.RoleExistsAsync(name))
                    {
                        var role = new IdentityRole<Guid>
                        {
                            Id = id,
                            Name = name,
                            NormalizedName = name.ToUpperInvariant()
                        };
                        await roleManager.CreateAsync(role);
                    }
                }

                await EnsureRoleAsync(SystemRoles.ParticipantId, SystemRoles.Participant);
                await EnsureRoleAsync(SystemRoles.AdminId, SystemRoles.Admin);
                await EnsureRoleAsync(SystemRoles.SuperAdminId, SystemRoles.SuperAdmin);
            }

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Приложение завершилось с фатальной ошибкой");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
