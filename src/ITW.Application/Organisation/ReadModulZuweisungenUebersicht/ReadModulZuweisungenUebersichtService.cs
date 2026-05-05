using ITW.Application.Organisation.Contracts;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Organisation.ReadModulZuweisungenUebersicht;

public sealed class ReadModulZuweisungenUebersichtService
{
    private readonly IModulZuweisungRepository _repository;
    private readonly ILogger<ReadModulZuweisungenUebersichtService> _logger;

    private static readonly ModulEmpfaengerDefinitionDto[] EmpfaengerDefinitionen =
    {
        new(OrganisationsbereichCode.Intensivtransport, BereichsrolleCode.Mitarbeiter),
        new(OrganisationsbereichCode.Intensivtransport, BereichsrolleCode.Wachleiter),
        new(OrganisationsbereichCode.Verwaltung, BereichsrolleCode.Verwaltungsmitarbeiter),
        new(OrganisationsbereichCode.Verwaltung, BereichsrolleCode.Vorstandsverwaltung),
        new(OrganisationsbereichCode.Vorstand, BereichsrolleCode.Vorstand)
    };

    public ReadModulZuweisungenUebersichtService(IModulZuweisungRepository repository, ILogger<ReadModulZuweisungenUebersichtService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReadModulZuweisungenUebersichtResult> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var alleZuweisungen = await _repository.GetAlleAsync(cancellationToken);

        var module = Enum.GetValues<ModulCode>()
            .Where(x => x != ModulCode.Unbekannt)
            .OrderBy(x => (int)x)
            .Select(modul => new ModulZuweisungMatrixZeileDto(
                modul,
                modul.GetAnzeigeName(),
                EmpfaengerDefinitionen
                    .Select(empfaenger => new ModulZuweisungMatrixZelleDto(
                        empfaenger.Bereich,
                        empfaenger.Rolle,
                        alleZuweisungen.Any(x =>
                            x.Modul == modul.ToDomain() &&
                            x.Bereich == empfaenger.Bereich.ToDomain() &&
                            x.Rolle == empfaenger.Rolle.ToDomain() &&
                            x.IstAktiv)))
                    .ToList()))
            .ToList();

        return ReadModulZuweisungenUebersichtResult.Erfolg(
            EmpfaengerDefinitionen,
            module);
    }
}