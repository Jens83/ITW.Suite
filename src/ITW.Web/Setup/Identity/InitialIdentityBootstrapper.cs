using ITW.Application.Organisation.Contracts;
using ITW.Application.Users.AssignArea;
using ITW.Infrastructure.Identity;
using ITW.Web.Configuration.Bootstrap;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ITW.Web.Setup.Identity;

public sealed class InitialIdentityBootstrapper
{
    private readonly IOptions<InitialIdentityBootstrapOptions> _options;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AssignUserToPrimaryAreaService _assignUserToPrimaryAreaService;
    private readonly ILogger<InitialIdentityBootstrapper> _logger;

    public InitialIdentityBootstrapper(
        IOptions<InitialIdentityBootstrapOptions> options,
        UserManager<ApplicationUser> userManager,
        AssignUserToPrimaryAreaService assignUserToPrimaryAreaService,
        ILogger<InitialIdentityBootstrapper> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        ArgumentNullException.ThrowIfNull(userManager);
        _userManager = userManager;
        ArgumentNullException.ThrowIfNull(assignUserToPrimaryAreaService);
        _assignUserToPrimaryAreaService = assignUserToPrimaryAreaService;
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var options = _options.Value;

        if (!options.Enabled)
        {
            _logger.LogInformation("Initialer Identity-Bootstrap ist deaktiviert.");
            return;
        }

        if (options.Users.Count == 0)
        {
            _logger.LogWarning(
                "Initialer Identity-Bootstrap ist aktiviert, aber es wurden keine Benutzer konfiguriert.");
            return;
        }

        foreach (var configuredUser in options.Users)
        {
            ValidateConfiguredUser(configuredUser);

            var bereich = ParseBereich(configuredUser.Bereich);
            var rolle = ParseRolle(configuredUser.Rolle);
            var fuehrungsverantwortung = ParseFuehrungsverantwortung(configuredUser.Fuehrungsverantwortung);

            var benutzername = configuredUser.Benutzername.Trim();
            var email = configuredUser.Email.Trim();

            var user = await _userManager.FindByNameAsync(benutzername);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = benutzername,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user, configuredUser.Passwort);

                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Der Benutzer '{benutzername}' konnte nicht angelegt werden: {FormatErrors(createResult.Errors)}");
                }

                _logger.LogInformation(
                    "Bootstrap-Benutzer '{Benutzername}' wurde neu angelegt.",
                    benutzername);
            }
            else
            {
                var needsUpdate = false;

                if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = email;
                    user.EmailConfirmed = true;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    var updateResult = await _userManager.UpdateAsync(user);

                    if (!updateResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Der Benutzer '{benutzername}' konnte nicht aktualisiert werden: {FormatErrors(updateResult.Errors)}");
                    }

                    _logger.LogInformation(
                        "Bootstrap-Benutzer '{BenutzerName}' wurde aktualisiert.",
                        benutzername);
                }
                else
                {
                    _logger.LogInformation(
                        "Bootstrap-Benutzer '{BenutzerName}' ist bereits vorhanden.",
                        benutzername);
                }
            }

            var assignResult = await _assignUserToPrimaryAreaService.ExecuteAsync(
                new AssignUserToPrimaryAreaCommand(
                    user.Id,
                    bereich,
                    rolle,
                    fuehrungsverantwortung,
                    BestehendePrimaereZuordnungErsetzen: true),
                cancellationToken);

            if (!assignResult.IsSuccess)
            {
                throw new InvalidOperationException(
                    $"Die Bereichszuordnung für '{benutzername}' konnte nicht gesetzt werden: {assignResult.ErrorMessage}");
            }

            _logger.LogInformation(
                "Bootstrap-Zuordnung für '{BenutzerName}' gesetzt: Bereich={Bereich}, Rolle={Rolle}, Führung={Fuehrungsverantwortung}",
                benutzername,
                bereich,
                rolle,
                fuehrungsverantwortung);
        }
    }

    private static void ValidateConfiguredUser(InitialIdentityUserOptions configuredUser)
    {
        if (configuredUser is null)
        {
            throw new InvalidOperationException("Ein Bootstrap-Benutzereintrag ist null.");
        }

        if (string.IsNullOrWhiteSpace(configuredUser.Benutzername))
        {
            throw new InvalidOperationException("Ein Bootstrap-Benutzer besitzt keinen Benutzernamen.");
        }

        if (string.IsNullOrWhiteSpace(configuredUser.Email))
        {
            throw new InvalidOperationException(
                $"Der Bootstrap-Benutzer '{configuredUser.Benutzername}' besitzt keine E-Mail-Adresse.");
        }

        if (string.IsNullOrWhiteSpace(configuredUser.Passwort))
        {
            throw new InvalidOperationException(
                $"Der Bootstrap-Benutzer '{configuredUser.Benutzername}' besitzt kein Passwort.");
        }

        if (string.IsNullOrWhiteSpace(configuredUser.Bereich))
        {
            throw new InvalidOperationException(
                $"Der Bootstrap-Benutzer '{configuredUser.Benutzername}' besitzt keinen Bereich.");
        }

        if (string.IsNullOrWhiteSpace(configuredUser.Rolle))
        {
            throw new InvalidOperationException(
                $"Der Bootstrap-Benutzer '{configuredUser.Benutzername}' besitzt keine Rolle.");
        }
    }

    private static OrganisationsbereichCode ParseBereich(string value)
    {
        return Normalize(value) switch
        {
            "INTENSIVTRANSPORT" => OrganisationsbereichCode.Intensivtransport,
            "VERWALTUNG" => OrganisationsbereichCode.Verwaltung,
            "GESCHAEFTSFUEHRUNG" => OrganisationsbereichCode.Vorstand,
            "VORSTAND" => OrganisationsbereichCode.Vorstand,
            _ => throw new InvalidOperationException(
                $"Der konfigurierte Bereich '{value}' ist unbekannt.")
        };
    }

    private static BereichsrolleCode ParseRolle(string value)
    {
        return Normalize(value) switch
        {
            "MITARBEITER" => BereichsrolleCode.Mitarbeiter,
            "ITWMITARBEITER" => BereichsrolleCode.Mitarbeiter,
            "WACHLEITER" => BereichsrolleCode.Wachleiter,

            "VERWALTUNGSMITARBEITER" => BereichsrolleCode.Verwaltungsmitarbeiter,
            "GESCHAEFTSFUEHRERVERWALTUNG" => BereichsrolleCode.Vorstandsverwaltung,
            "VERWALTUNGSLEITUNG" => BereichsrolleCode.Vorstandsverwaltung,
            "VORSTANDSVERALTUNG" => BereichsrolleCode.Vorstandsverwaltung,

            "FUEHRUNGSEBENE" => BereichsrolleCode.Vorstand,
            "GESCHAEFTSFUEHRUNG" => BereichsrolleCode.Vorstand,
            "VORSTAND" => BereichsrolleCode.Vorstand,

            _ => throw new InvalidOperationException(
                $"Die konfigurierte Rolle '{value}' ist unbekannt.")
        };
    }

    private static FuehrungsverantwortungCode ParseFuehrungsverantwortung(string value)
    {
        return Normalize(value) switch
        {
            "" => FuehrungsverantwortungCode.Keine,
            "KEINE" => FuehrungsverantwortungCode.Keine,
            "OPERATIVELEITUNG" => FuehrungsverantwortungCode.OperativeLeitung,
            "BEREICHSLEITUNG" => FuehrungsverantwortungCode.Bereichsleitung,
            "UEBERGEORDNETELEITUNG" => FuehrungsverantwortungCode.UebergeordneteLeitung,
            _ => throw new InvalidOperationException(
                $"Die konfigurierte Führungsverantwortung '{value}' ist unbekannt.")
        };
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .Trim()
            .ToUpperInvariant()
            .Replace("Ä", "AE", StringComparison.Ordinal)
            .Replace("Ö", "OE", StringComparison.Ordinal)
            .Replace("Ü", "UE", StringComparison.Ordinal)
            .Replace("ß", "SS", StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal);
    }

    private static string FormatErrors(IEnumerable<IdentityError> errors)
    {
        return string.Join("; ", errors.Select(x => x.Description));
    }
}