using ITW.Application.Abstractions.DateTime;
using ITW.Application.Organisation.Contracts;
using ITW.Domain.Organisation.Entities;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Organisation.SetModulZuweisungStatus;

public sealed class SetModulZuweisungStatusService(
    IModulZuweisungRepository repository,
    IDateTimeProvider dateTimeProvider,
    ILogger<SetModulZuweisungStatusService> logger)
{
    public async Task<SetModulZuweisungStatusResult> ExecuteAsync(
        SetModulZuweisungStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("UseCase {UseCase} begonnen", nameof(SetModulZuweisungStatusService));

        if (command.Modul == ModulCode.Unbekannt)
        {
            logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Modul ungültig", nameof(SetModulZuweisungStatusService));
            return SetModulZuweisungStatusResult.Fehler("Das Modul ist ungültig.");
        }

        if (command.Bereich == OrganisationsbereichCode.Unbekannt)
        {
            logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Bereich ungültig", nameof(SetModulZuweisungStatusService));
            return SetModulZuweisungStatusResult.Fehler("Der Bereich ist ungültig.");
        }

        if (command.Rolle == BereichsrolleCode.Unbekannt)
        {
            logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Rolle ungültig", nameof(SetModulZuweisungStatusService));
            return SetModulZuweisungStatusResult.Fehler("Die Rolle ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.BenutzerId))
        {
            logger.LogWarning("UseCase {UseCase} fehlgeschlagen: BenutzerId leer", nameof(SetModulZuweisungStatusService));
            return SetModulZuweisungStatusResult.Fehler("Die BenutzerId darf nicht leer sein.");
        }

        var bestehend = await repository.GetByModulBereichRolleAsync(
            command.Modul.ToDomain(),
            command.Bereich.ToDomain(),
            command.Rolle.ToDomain(),
            cancellationToken);

        if (bestehend is null)
        {
            if (!command.IstAktiv)
            {
                logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SetModulZuweisungStatusService));
                return SetModulZuweisungStatusResult.Erfolg();
            }

            var neu = new ModulZuweisung(
                Guid.NewGuid(),
                command.Modul.ToDomain(),
                command.Bereich.ToDomain(),
                command.Rolle.ToDomain(),
                command.BenutzerId.Trim(),
                dateTimeProvider.UtcNow);

            await repository.AddAsync(neu, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SetModulZuweisungStatusService));
            return SetModulZuweisungStatusResult.Erfolg();
        }

        if (command.IstAktiv)
        {
            bestehend.Aktivieren(command.BenutzerId.Trim(), dateTimeProvider.UtcNow);
        }
        else
        {
            bestehend.Deaktivieren(command.BenutzerId.Trim(), dateTimeProvider.UtcNow);
        }

        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SetModulZuweisungStatusService));
        return SetModulZuweisungStatusResult.Erfolg();
    }
}
