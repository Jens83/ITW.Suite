using ITW.Application.Abstractions.DateTime;
using ITW.Application.Organisation.Contracts;
using ITW.Domain.Organisation.Entities;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Organisation.SetModulZuweisungStatus;

public sealed class SetModulZuweisungStatusService
{
    private readonly IModulZuweisungRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<SetModulZuweisungStatusService> _logger;

    public SetModulZuweisungStatusService(
        IModulZuweisungRepository repository,
        IDateTimeProvider dateTimeProvider,
        ILogger<SetModulZuweisungStatusService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SetModulZuweisungStatusResult> ExecuteAsync(
        SetModulZuweisungStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(SetModulZuweisungStatusService));

        if (command.Modul == ModulCode.Unbekannt)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Modul ungültig", nameof(SetModulZuweisungStatusService));
            return SetModulZuweisungStatusResult.Fehler("Das Modul ist ungültig.");
        }

        if (command.Bereich == OrganisationsbereichCode.Unbekannt)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Bereich ungültig", nameof(SetModulZuweisungStatusService));
            return SetModulZuweisungStatusResult.Fehler("Der Bereich ist ungültig.");
        }

        if (command.Rolle == BereichsrolleCode.Unbekannt)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Rolle ungültig", nameof(SetModulZuweisungStatusService));
            return SetModulZuweisungStatusResult.Fehler("Die Rolle ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.BenutzerId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: BenutzerId leer", nameof(SetModulZuweisungStatusService));
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
                _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SetModulZuweisungStatusService));
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

            _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SetModulZuweisungStatusService));
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

        _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SetModulZuweisungStatusService));
        return SetModulZuweisungStatusResult.Erfolg();
    }
}