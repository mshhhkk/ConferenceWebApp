using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Persistence.Repositories.Realization;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceWebApp.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IReportsRepository, ReportsRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<ICommitteRepository, CommitteRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<ITwoFactorCodeRepository, TwoFactorCodeRepository>();
        return services;
    }
}
