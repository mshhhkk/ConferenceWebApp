using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Application.Interfaces.Services.Admin;
using ConferenceWebApp.Infrastructure.Services.Realization;
using ConferenceWebApp.Infrastructure.Services.Realization.Admin;
using Microsoft.Extensions.DependencyInjection;
namespace ConferenceWebApp.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IExtendedThesisService, ExtendedThesisService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPersonalAccountService, PersonalAccountService>();
        services.AddScoped<IReportsReferralService, ReportsReferralService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IUserSecurityService, UserSecurityService>();
        services.AddScoped<ICommitteeService, CommitteeService>();
        services.AddScoped<IAdminPaymentService, AdminPaymentService>();
        services.AddScoped<IAdminCommitteeService, AdminCommitteeService>();
        services.AddScoped<IReportAdminService, ReportAdminService>();
        services.AddScoped<IScheduleAdminService, ScheduleAdminService>();
        services.AddScoped<IUserProfileService, UserService>();
        return services;
    }
}
