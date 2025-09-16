using ConferenceWebApp.Application.Extensions;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Infrastructure.Extensions;
using ConferenceWebApp.Persistence;
using ConferenceWebApp.Persistence.Extensions;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace ConferenceWebApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        IConfigurationBuilder configBuild = new ConfigurationBuilder()
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        IConfiguration configuration = configBuild.Build();

        builder.Services.AddDatabase(builder.Configuration);
        builder.Services.AddApplicationServices();
        builder.Services.AddPersistence();
        


        builder.Services.AddFluentValidationAutoValidation(options =>
        {
            // Можно настроить, например, чтобы пустые строки не воспринимались как null
            options.DisableDataAnnotationsValidation = false;
        });

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

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Auth/Login";
            options.LogoutPath = "/Auth/Logout";
            options.AccessDeniedPath = "/Auth/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(20);
            options.Cookie.HttpOnly = true;
            options.SlidingExpiration = true;
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
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            var roles = new[] { "Participant", "Admin", "SuperAdmin" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role, NormalizedName = role.ToUpper() });
                }
            }
        }

        await app.RunAsync();
    }
}