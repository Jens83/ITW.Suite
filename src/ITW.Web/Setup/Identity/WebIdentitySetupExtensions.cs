using ITW.Infrastructure.Identity;
using ITW.Infrastructure.Persistence.DbContexts;
using ITW.Web.Configuration.Bootstrap;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ITW.Web.Setup.Identity;

public static class WebIdentitySetupExtensions
{
    public static IServiceCollection AddWebIdentitySetup(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<InitialIdentityBootstrapOptions>(
            configuration.GetSection("IdentityBootstrap"));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<PlatformDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services
            .AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/KeinZugriff";
            options.LogoutPath = "/Account/Logout";
        });

        services.AddAuthorization();

        return services;
    }
}