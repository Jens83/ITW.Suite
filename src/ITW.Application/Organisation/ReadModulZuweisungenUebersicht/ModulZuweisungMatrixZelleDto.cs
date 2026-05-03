using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Organisation.ReadModulZuweisungenUebersicht;

public sealed record ModulZuweisungMatrixZelleDto(
    OrganisationsbereichCode Bereich,
    BereichsrolleCode Rolle,
    bool IstAktiv);