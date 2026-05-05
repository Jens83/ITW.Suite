// Datei: src/ITW.Application/Users/ReadOffenePasswortResetAnfragen/ReadOffenePasswortResetAnfragenService.cs
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Users.ReadOffenePasswortResetAnfragen;

public sealed class ReadOffenePasswortResetAnfragenService
{
    private readonly IPasswortResetAnfrageRepository _passwortResetAnfrageRepository;
    private readonly ILogger<ReadOffenePasswortResetAnfragenService> _logger;

    public ReadOffenePasswortResetAnfragenService(
        IPasswortResetAnfrageRepository passwortResetAnfrageRepository,
        ILogger<ReadOffenePasswortResetAnfragenService> logger)
    {
        _passwortResetAnfrageRepository = passwortResetAnfrageRepository
            ?? throw new ArgumentNullException(nameof(passwortResetAnfrageRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReadOffenePasswortResetAnfragenResult> ExecuteAsync(
        ReadOffenePasswortResetAnfragenQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.Bereich == OrganisationsbereichCode.Unbekannt)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Bereich ungültig", nameof(ReadOffenePasswortResetAnfragenService));
            return ReadOffenePasswortResetAnfragenResult.Fehler(
                "Der Bereich für die Passwort-Reset-Anfragen ist ungültig.");
        }

        var anfragen = await _passwortResetAnfrageRepository.GetOffeneAnfragenByBereichAsync(
            query.Bereich.ToDomain(),
            cancellationToken);

        var result = anfragen
            .Select(x => new OffenePasswortResetAnfrageDto(
                x.Id,
                x.UserId,
                x.Benutzername,
                x.Vorname,
                x.Nachname,
                x.AngefordertAm))
            .ToList();

        return ReadOffenePasswortResetAnfragenResult.Erfolg(result);
    }
}