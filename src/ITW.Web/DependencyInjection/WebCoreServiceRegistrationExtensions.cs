using ITW.Web.Logging;
using ITW.Web.Security.CurrentUser;
using ITW.Web.Setup.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ITW.Web.DependencyInjection;

public static class WebCoreServiceRegistrationExtensions
{
    public static IServiceCollection AddWebCoreServices(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserContextAccessor, CurrentUserContextAccessor>();
        services.AddScoped<InitialIdentityBootstrapper>();
        services.AddSingleton<ILogEintragService, ClefLogEintragService>();

        return services;
    }
}