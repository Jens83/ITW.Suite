using ITW.Application.Organisation.Contracts;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Organisation.ReadAktiveModule;

public sealed class ReadAktiveModuleService(
    IModulZuweisungRepository repository,
    ILogger<ReadAktiveModuleService> logger)
{
    public async Task<ReadAktiveModuleResult> ExecuteAsync(
        OrganisationsbereichCode bereich,
        BereichsrolleCode rolle,
        CancellationToken cancellationToken = default)
    {
        if (bereich == OrganisationsbereichCode.Unbekannt)
        {
            logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Bereich ungültig", nameof(ReadAktiveModuleService));
            return ReadAktiveModuleResult.Fehler("Der Bereich ist ungültig.");
        }

        if (rolle == BereichsrolleCode.Unbekannt)
        {
            logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Rolle ungültig", nameof(ReadAktiveModuleService));
            return ReadAktiveModuleResult.Fehler("Die Rolle ist ungültig.");
        }

        var zuweisungen = await repository.GetAktiveModuleFuerBereichUndRolleAsync(
            bereich.ToDomain(),
            rolle.ToDomain(),
            cancellationToken);

        var module = zuweisungen
            .Select(x => x.Modul.ToApplication())
            .Distinct()
            .OrderBy(x => (int)x)
            .ToList();

        return ReadAktiveModuleResult.Erfolg(module);
    }
}
