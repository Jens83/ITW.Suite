using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Organisation.ReadAktiveModule;

public sealed class ReadAktiveModuleService
{
    private readonly IModulZuweisungRepository _repository;

    public ReadAktiveModuleService(IModulZuweisungRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ReadAktiveModuleResult> ExecuteAsync(
        OrganisationsbereichCode bereich,
        BereichsrolleCode rolle,
        CancellationToken cancellationToken = default)
    {
        if (bereich == OrganisationsbereichCode.Unbekannt)
        {
            return ReadAktiveModuleResult.Fehler("Der Bereich ist ungültig.");
        }

        if (rolle == BereichsrolleCode.Unbekannt)
        {
            return ReadAktiveModuleResult.Fehler("Die Rolle ist ungültig.");
        }

        var zuweisungen = await _repository.GetAktiveModuleFuerBereichUndRolleAsync(
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