using ITW.Application.Abstractions.DateTime;
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;
using ITW.Domain.Organisation.Entities;

namespace ITW.Application.Users.AssignArea;

public sealed class AssignUserToPrimaryAreaService
{
    private readonly IBenutzerBereichszuordnungRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AssignUserToPrimaryAreaService(
        IBenutzerBereichszuordnungRepository repository,
        IDateTimeProvider dateTimeProvider)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<AssignUserToPrimaryAreaResult> ExecuteAsync(
        AssignUserToPrimaryAreaCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.UserId))
        {
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
                return AssignUserToPrimaryAreaResult.Erfolg(bestehendeZuordnung.Id);
            }

            if (!command.BestehendePrimaereZuordnungErsetzen)
            {
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

        return AssignUserToPrimaryAreaResult.Erfolg(neueZuordnung.Id);
    }
}