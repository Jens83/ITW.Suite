// Datei: src/ITW.Application/Users/CountOffenePasswortResetAnfragen/CountOffenePasswortResetAnfragenService.cs
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Users.CountOffenePasswortResetAnfragen;

public sealed class CountOffenePasswortResetAnfragenService
{
    private readonly IPasswortResetAnfrageRepository _passwortResetAnfrageRepository;
    private readonly ILogger<CountOffenePasswortResetAnfragenService> _logger;

    public CountOffenePasswortResetAnfragenService(
        IPasswortResetAnfrageRepository passwortResetAnfrageRepository,
        ILogger<CountOffenePasswortResetAnfragenService> logger)
    {
        ArgumentNullException.ThrowIfNull(passwortResetAnfrageRepository);
        _passwortResetAnfrageRepository = passwortResetAnfrageRepository;
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<CountOffenePasswortResetAnfragenResult> ExecuteAsync(
        CountOffenePasswortResetAnfragenQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.Bereich == OrganisationsbereichCode.Unbekannt)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Bereich ungültig", nameof(CountOffenePasswortResetAnfragenService));
            return CountOffenePasswortResetAnfragenResult.Fehler(
                "Der Bereich für die Passwort-Reset-Zählung ist ungültig.");
        }

        var anzahl = await _passwortResetAnfrageRepository.CountOffeneAnfragenByBereichAsync(
            query.Bereich.ToDomain(),
            cancellationToken);

        return CountOffenePasswortResetAnfragenResult.Erfolg(anzahl);
    }
}