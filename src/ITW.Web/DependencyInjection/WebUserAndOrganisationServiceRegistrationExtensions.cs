using ITW.Application.Organisation.VisibilityScopes;
using ITW.Application.Users.ActivateUser;
using ITW.Application.Users.AssignArea;
using ITW.Application.Users.ChangeAreaRole;
using ITW.Application.Users.CountOffenePasswortResetAnfragen;
using ITW.Application.Users.CreateUser;
using ITW.Application.Users.LockUser;
using ITW.Application.Users.ReadNichtZugeordneteBenutzerkonten;
using ITW.Application.Users.ReadOffenePasswortResetAnfrageDetail;
using ITW.Application.Users.ReadOffenePasswortResetAnfragen;
using ITW.Application.Users.ReadUserOrganisationskontext;
using ITW.Application.Users.ReadUsersByScope;
using ITW.Application.Users.RequestPasswordReset;
using ITW.Application.Users.SetzeTemporaeresPasswort;
using Microsoft.Extensions.DependencyInjection;

namespace ITW.Web.DependencyInjection;

public static class WebUserAndOrganisationServiceRegistrationExtensions
{
    public static IServiceCollection AddWebUserAndOrganisationServices(this IServiceCollection services)
    {
        services.AddScoped<BenutzerSichtbarkeitsScopeErmittler>();
        services.AddScoped<ReadUsersByScopeService>();
        services.AddScoped<AssignUserToPrimaryAreaService>();
        services.AddScoped<ChangeUserAreaRoleService>();
        services.AddScoped<ReadUserOrganisationskontextService>();
        services.AddScoped<ReadNichtZugeordneteBenutzerkontenService>();
        services.AddScoped<CreateBenutzerkontoService>();
        services.AddScoped<LockUserService>();
        services.AddScoped<ActivateUserService>();

        services.AddScoped<SubmitPasswortResetAnfrageService>();
        services.AddScoped<ReadOffenePasswortResetAnfragenService>();
        services.AddScoped<CountOffenePasswortResetAnfragenService>();
        services.AddScoped<ReadOffenePasswortResetAnfrageDetailService>();
        services.AddScoped<SetzeTemporaeresPasswortService>();

        return services;
    }
}