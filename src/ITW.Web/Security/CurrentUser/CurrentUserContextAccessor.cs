using System.Security.Claims;
using ITW.Application.Organisation.ReadAktiveModule;
using ITW.Application.Users.ReadUserOrganisationskontext;

namespace ITW.Web.Security.CurrentUser;

public sealed class CurrentUserContextAccessor : ICurrentUserContextAccessor
{
    private const string CacheKey = "__ITW_CurrentUserContextLookupResult";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ReadUserOrganisationskontextService _readUserOrganisationskontextService;
    private readonly ReadAktiveModuleService _readAktiveModuleService;

    public CurrentUserContextAccessor(
        IHttpContextAccessor httpContextAccessor,
        ReadUserOrganisationskontextService readUserOrganisationskontextService,
        ReadAktiveModuleService readAktiveModuleService)
    {
        _httpContextAccessor = httpContextAccessor
            ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _readUserOrganisationskontextService = readUserOrganisationskontextService
            ?? throw new ArgumentNullException(nameof(readUserOrganisationskontextService));
        _readAktiveModuleService = readAktiveModuleService
            ?? throw new ArgumentNullException(nameof(readAktiveModuleService));
    }

    public async Task<CurrentUserContextLookupResult> GetCurrentAsync(
        CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return CurrentUserContextLookupResult.Fehler(
                "Es ist kein HttpContext verfügbar.");
        }

        if (httpContext.Items.TryGetValue(CacheKey, out var cachedValue) &&
            cachedValue is CurrentUserContextLookupResult cachedResult)
        {
            return cachedResult;
        }

        var result = await LadeCurrentUserAsync(httpContext, cancellationToken);
        httpContext.Items[CacheKey] = result;

        return result;
    }

    private async Task<CurrentUserContextLookupResult> LadeCurrentUserAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var principal = httpContext.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return CurrentUserContextLookupResult.Fehler(
                "Es ist kein authentifizierter Mitarbeiter vorhanden.");
        }

        var userId =
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            principal.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return CurrentUserContextLookupResult.Fehler(
                "Die UserId des aktuellen Mitarbeiters konnte nicht aus der Datenbank gelesen werden.");
        }

        var organisationskontextResult = await _readUserOrganisationskontextService.ExecuteAsync(
            new ReadUserOrganisationskontextQuery(userId),
            cancellationToken);

        if (!organisationskontextResult.IsSuccess || organisationskontextResult.Benutzer is null)
        {
            return CurrentUserContextLookupResult.Fehler(
                organisationskontextResult.ErrorMessage ?? "Der Mitarbeiter konnte nicht geladen werden.");
        }

        var modulResult = await _readAktiveModuleService.ExecuteAsync(
            organisationskontextResult.Benutzer.Bereich,
            organisationskontextResult.Benutzer.Rolle,
            cancellationToken);

        var aktiveModule = modulResult.IsSuccess
            ? modulResult.Module
            : Array.Empty<ITW.Application.Organisation.Contracts.ModulCode>();

        var currentUser = new CurrentUserContext(
            organisationskontextResult.Benutzer.UserId,
            organisationskontextResult.Benutzer.Bereich,
            organisationskontextResult.Benutzer.Rolle,
            organisationskontextResult.Benutzer.Fuehrungsverantwortung,
            aktiveModule);

        return CurrentUserContextLookupResult.Erfolg(currentUser);
    }
}