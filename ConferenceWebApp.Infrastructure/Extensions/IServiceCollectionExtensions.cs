using Microsoft.EntityFrameworkCore;
using ConferenceWebApp.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using ConferenceWebApp.Infrastructure.Services.Realization;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Infrastructuren.Services;

namespace ConferenceWebApp.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null
                );
            });
        });

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string rootPath)
    {
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddSingleton<IFileService>(provider => new FileService(rootPath));
        return services;
    }
}

