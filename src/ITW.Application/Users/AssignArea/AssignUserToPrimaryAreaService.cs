using ITW.Application.Abstractions.DateTime;
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;
using ITW.Domain.Organisation.Entities;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Users.AssignArea;

public sealed class AssignUserToPrimaryAreaService
{
    private readonly IBenutzerBereichszuordnungRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<AssignUserToPrimaryAreaService> _logger;

    public AssignUserToPrimaryAreaService(
        IBenutzerBereichszuordnungRepository repository,
        IDateTimeProvider dateTimeProvider,
        ILogger<AssignUserToPrimaryAreaService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _dateTimeProvider = dateTimeProvider;
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<AssignUserToPrimaryAreaResult> ExecuteAsync(
        AssignUserToPrimaryAreaCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(AssignUserToPrimaryAreaService));

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(AssignUserToPrimaryAreaService));
            return AssignUserToPrimaryAreaResult.Fehler("Die UserId darf nicht leer sein.");
        }

        var bestehendeZuordnung = await _repository.GetAktivePrimaereZuordnungAsync(
            command.UserId,
            cancellationToken);

        if (bestehendeZuordnung is not null)
        {
            var istIdentisch =
                bestehendeZuordnung.Bereich == command.Bereich.ToDomain() &&
                bestehendeZuordnung.Rolle == command.Rolle.ToDomain() &&
                bestehendeZuordnung.Fuehrungsverantwortung == command.Fuehrungsverantwortung.ToDomain();

            if (istIdentisch)
            {
                _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(AssignUserToPrimaryAreaService));
                return AssignUserToPrimaryAreaResult.Erfolg(bestehendeZuordnung.Id);
            }

            if (!command.BestehendePrimaereZuordnungErsetzen)
            {
                _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(AssignUserToPrimaryAreaService), "Bestehende primäre Zuordnung vorhanden");
                return AssignUserToPrimaryAreaResult.Fehler(
                    "Der Benutzer besitzt bereits eine aktive primäre Bereichszuordnung.");
            }

            bestehendeZuordnung.Deaktivieren(_dateTimeProvider.UtcNow);
        }

        var neueZuordnung = new BenutzerBereichszuordnung(
            Guid.NewGuid(),
            command.UserId,
            command.Bereich.ToDomain(),
            command.Rolle.ToDomain(),
            command.Fuehrungsverantwortung.ToDomain(),
            true,
            _dateTimeProvider.UtcNow);

        await _repository.AddAsync(neueZuordnung, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(AssignUserToPrimaryAreaService));
        return AssignUserToPrimaryAreaResult.Erfolg(neueZuordnung.Id);
    }
}