using ITW.Web.Areas.Intensivtransport.Services.Dashboard;
using Microsoft.Extensions.DependencyInjection;

namespace ITW.Web.DependencyInjection;

public static class WebDashboardServiceRegistrationExtensions
{
    public static IServiceCollection AddWebDashboardServices(this IServiceCollection services)
    {
        services.AddScoped<GetItwDashboardDataService>();

        return services;
    }
}
