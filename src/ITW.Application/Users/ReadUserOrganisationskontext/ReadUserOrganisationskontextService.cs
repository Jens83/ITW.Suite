using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Users.ReadUserOrganisationskontext;

public sealed class ReadUserOrganisationskontextService
{
    private readonly IBenutzerBereichszuordnungRepository _repository;
    private readonly ILogger<ReadUserOrganisationskontextService> _logger;

    public ReadUserOrganisationskontextService(
        IBenutzerBereichszuordnungRepository repository,
        ILogger<ReadUserOrganisationskontextService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReadUserOrganisationskontextResult> ExecuteAsync(
        ReadUserOrganisationskontextQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (string.IsNullOrWhiteSpace(query.UserId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(ReadUserOrganisationskontextService));
            return ReadUserOrganisationskontextResult.Fehler(
                "Die UserId des Mitarbeiters ist leer.");
        }

        var zuordnung = await _repository.GetAktivePrimaereZuordnungAsync(
            query.UserId,
            cancellationToken);

        if (zuordnung is null)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(ReadUserOrganisationskontextService), "Keine aktive primäre Bereichszuordnung gefunden");
            return ReadUserOrganisationskontextResult.Fehler(
                "Für den Mitarbeiter wurde keine aktive primäre Bereichszuordnung gefunden.");
        }

        var benutzer = new BenutzerOrganisationskontextDto(
            zuordnung.UserId,
            zuordnung.Bereich.ToApplication(),
            zuordnung.Rolle.ToApplication(),
            zuordnung.Fuehrungsverantwortung.ToApplication());

        return ReadUserOrganisationskontextResult.Erfolg(benutzer);
    }
}