// Datei: src/ITW.Application/Users/RequestPasswordReset/SubmitPasswortResetAnfrageService.cs
using ITW.Application.Abstractions.DateTime;
using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Security.Entities;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<SubmitPasswortResetAnfrageService> _logger;

    public SubmitPasswortResetAnfrageService(
        IPasswortResetBenutzerLookupRepository benutzerLookupRepository,
        IAllgemeinesMitarbeiterprofilRepository allgemeinesMitarbeiterprofilRepository,
        IBenutzerBereichszuordnungRepository benutzerBereichszuordnungRepository,
        IPasswortResetAnfrageRepository passwortResetAnfrageRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<SubmitPasswortResetAnfrageService> logger)
    {
        ArgumentNullException.ThrowIfNull(benutzerLookupRepository);
        _benutzerLookupRepository = benutzerLookupRepository;
        ArgumentNullException.ThrowIfNull(allgemeinesMitarbeiterprofilRepository);
        _allgemeinesMitarbeiterprofilRepository = allgemeinesMitarbeiterprofilRepository;
        ArgumentNullException.ThrowIfNull(benutzerBereichszuordnungRepository);
        _benutzerBereichszuordnungRepository = benutzerBereichszuordnungRepository;
        ArgumentNullException.ThrowIfNull(passwortResetAnfrageRepository);
        _passwortResetAnfrageRepository = passwortResetAnfrageRepository;
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _dateTimeProvider = dateTimeProvider;
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<SubmitPasswortResetAnfrageResult> ExecuteAsync(
        SubmitPasswortResetAnfrageCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(SubmitPasswortResetAnfrageService));

        if (string.IsNullOrWhiteSpace(command.Benutzername))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Benutzername leer", nameof(SubmitPasswortResetAnfrageService));
            return SubmitPasswortResetAnfrageResult.Fehler("Der Benutzername ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.Vorname))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Vorname leer", nameof(SubmitPasswortResetAnfrageService));
            return SubmitPasswortResetAnfrageResult.Fehler("Der Vorname ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.Nachname))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Nachname leer", nameof(SubmitPasswortResetAnfrageService));
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
            _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SubmitPasswortResetAnfrageService));
            return SubmitPasswortResetAnfrageResult.Erfolg(GenerischeBestaetigung);
        }

        var profil = await _allgemeinesMitarbeiterprofilRepository.GetByUserIdAsync(
            konto.UserId,
            cancellationToken);

        if (profil is null)
        {
            _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SubmitPasswortResetAnfrageService));
            return SubmitPasswortResetAnfrageResult.Erfolg(GenerischeBestaetigung);
        }

        if (!IstGleicherName(profil.Vorname, vorname) ||
            !IstGleicherName(profil.Nachname, nachname))
        {
            _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SubmitPasswortResetAnfrageService));
            return SubmitPasswortResetAnfrageResult.Erfolg(GenerischeBestaetigung);
        }

        var zuordnung = await _benutzerBereichszuordnungRepository.GetAktivePrimaereZuordnungAsync(
            konto.UserId,
            cancellationToken);

        if (zuordnung is null)
        {
            _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SubmitPasswortResetAnfrageService));
            return SubmitPasswortResetAnfrageResult.Erfolg(GenerischeBestaetigung);
        }

        var offeneAnfrage = await _passwortResetAnfrageRepository.GetOffeneAnfrageFuerUserAsync(
            konto.UserId,
            cancellationToken);

        if (offeneAnfrage is not null)
        {
            _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SubmitPasswortResetAnfrageService));
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

        _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SubmitPasswortResetAnfrageService));
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