// Datei: src/ITW.Application/Users/RequestPasswordReset/SubmitPasswortResetAnfrageService.cs
using ITW.Application.Abstractions.DateTime;
using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Security.Entities;

namespace ITW.Application.Users.RequestPasswordReset;

public sealed class SubmitPasswortResetAnfrageService
{
    private const string GenerischeBestaetigung =
        "Wenn die Angaben zu einem aktiven Benutzerkonto passen, wurde eine Passwort-Reset-Anfrage für den zuständigen Bereich erfasst.";

    private readonly IPasswortResetBenutzerLookupRepository _benutzerLookupRepository;
    private readonly IAllgemeinesMitarbeiterprofilRepository _allgemeinesMitarbeiterprofilRepository;
    private readonly IBenutzerBereichszuordnungRepository _benutzerBereichszuordnungRepository;
    private readonly IPasswortResetAnfrageRepository _passwortResetAnfrageRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SubmitPasswortResetAnfrageService(
        IPasswortResetBenutzerLookupRepository benutzerLookupRepository,
        IAllgemeinesMitarbeiterprofilRepository allgemeinesMitarbeiterprofilRepository,
        IBenutzerBereichszuordnungRepository benutzerBereichszuordnungRepository,
        IPasswortResetAnfrageRepository passwortResetAnfrageRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _benutzerLookupRepository = benutzerLookupRepository
            ?? throw new ArgumentNullException(nameof(benutzerLookupRepository));
        _allgemeinesMitarbeiterprofilRepository = allgemeinesMitarbeiterprofilRepository
            ?? throw new ArgumentNullException(nameof(allgemeinesMitarbeiterprofilRepository));
        _benutzerBereichszuordnungRepository = benutzerBereichszuordnungRepository
            ?? throw new ArgumentNullException(nameof(benutzerBereichszuordnungRepository));
        _passwortResetAnfrageRepository = passwortResetAnfrageRepository
            ?? throw new ArgumentNullException(nameof(passwortResetAnfrageRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<SubmitPasswortResetAnfrageResult> ExecuteAsync(
        SubmitPasswortResetAnfrageCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Benutzername))
        {
            return SubmitPasswortResetAnfrageResult.Fehler("Der Benutzername ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.Vorname))
        {
            return SubmitPasswortResetAnfrageResult.Fehler("Der Vorname ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.Nachname))
        {
            return SubmitPasswortResetAnfrageResult.Fehler("Der Nachname ist erforderlich.");
        }

        var benutzername = command.Benutzername.Trim();
        var vorname = command.Vorname.Trim();
        var nachname = command.Nachname.Trim();

        var konto = await _benutzerLookupRepository.GetByBenutzernameAsync(
            benutzername,
            cancellationToken);

        if (konto is null)
        {
            return SubmitPasswortResetAnfrageResult.Erfolg(GenerischeBestaetigung);
        }

        var profil = await _allgemeinesMitarbeiterprofilRepository.GetByUserIdAsync(
            konto.UserId,
            cancellationToken);

        if (profil is null)
        {
            return SubmitPasswortResetAnfrageResult.Erfolg(GenerischeBestaetigung);
        }

        if (!IstGleicherName(profil.Vorname, vorname) ||
            !IstGleicherName(profil.Nachname, nachname))
        {
            return SubmitPasswortResetAnfrageResult.Erfolg(GenerischeBestaetigung);
        }

        var zuordnung = await _benutzerBereichszuordnungRepository.GetAktivePrimaereZuordnungAsync(
            konto.UserId,
            cancellationToken);

        if (zuordnung is null)
        {
            return SubmitPasswortResetAnfrageResult.Erfolg(GenerischeBestaetigung);
        }

        var offeneAnfrage = await _passwortResetAnfrageRepository.GetOffeneAnfrageFuerUserAsync(
            konto.UserId,
            cancellationToken);

        if (offeneAnfrage is not null)
        {
            return SubmitPasswortResetAnfrageResult.Erfolg(GenerischeBestaetigung);
        }

        var anfrage = new PasswortResetAnfrage(
            Guid.NewGuid(),
            konto.UserId,
            konto.Benutzername,
            vorname,
            nachname,
            zuordnung.Bereich,
            _dateTimeProvider.UtcNow);

        await _passwortResetAnfrageRepository.AddAsync(anfrage, cancellationToken);
        await _passwortResetAnfrageRepository.SaveChangesAsync(cancellationToken);

        return SubmitPasswortResetAnfrageResult.Erfolg(GenerischeBestaetigung);
    }

    private static bool IstGleicherName(
        string gespeicherterWert,
        string eingegebenerWert)
    {
        return string.Equals(
            gespeicherterWert?.Trim(),
            eingegebenerWert?.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }
}