using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Organisation.ReadModulZuweisungenUebersicht;

public sealed record ModulEmpfaengerDefinitionDto(
    OrganisationsbereichCode Bereich,
    BereichsrolleCode Rolle);