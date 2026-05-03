using ITW.Application.Abstractions.DateTime;
using ITW.Application.Organisation.Contracts;
using ITW.Domain.Organisation.Entities;

namespace ITW.Application.Organisation.SetModulZuweisungStatus;

public sealed class SetModulZuweisungStatusService
{
    private readonly IModulZuweisungRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SetModulZuweisungStatusService(
        IModulZuweisungRepository repository,
        IDateTimeProvider dateTimeProvider)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<SetModulZuweisungStatusResult> ExecuteAsync(
        SetModulZuweisungStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Modul == ModulCode.Unbekannt)
        {
            return SetModulZuweisungStatusResult.Fehler("Das Modul ist ungültig.");
        }

        if (command.Bereich == OrganisationsbereichCode.Unbekannt)
        {
            return SetModulZuweisungStatusResult.Fehler("Der Bereich ist ungültig.");
        }

        if (command.Rolle == BereichsrolleCode.Unbekannt)
        {
            return SetModulZuweisungStatusResult.Fehler("Die Rolle ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.BenutzerId))
        {
            return SetModulZuweisungStatusResult.Fehler("Die BenutzerId darf nicht leer sein.");
        }

        var bestehend = await _repository.GetByModulBereichRolleAsync(
            command.Modul.ToDomain(),
            command.Bereich.ToDomain(),
            command.Rolle.ToDomain(),
            cancellationToken);

        if (bestehend is null)
        {
            if (!command.IstAktiv)
            {
                return SetModulZuweisungStatusResult.Erfolg();
            }

            var neu = new ModulZuweisung(
                Guid.NewGuid(),
                command.Modul.ToDomain(),
                command.Bereich.ToDomain(),
                command.Rolle.ToDomain(),
                command.BenutzerId.Trim(),
                _dateTimeProvider.UtcNow);

            await _repository.AddAsync(neu, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return SetModulZuweisungStatusResult.Erfolg();
        }

        if (command.IstAktiv)
        {
            bestehend.Aktivieren(command.BenutzerId.Trim(), _dateTimeProvider.UtcNow);
        }
        else
        {
            bestehend.Deaktivieren(command.BenutzerId.Trim(), _dateTimeProvider.UtcNow);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return SetModulZuweisungStatusResult.Erfolg();
    }
}