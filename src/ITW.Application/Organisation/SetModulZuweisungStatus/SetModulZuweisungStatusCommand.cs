using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Organisation.SetModulZuweisungStatus;

public sealed record SetModulZuweisungStatusCommand(
    ModulCode Modul,
    OrganisationsbereichCode Bereich,
    BereichsrolleCode Rolle,
    bool IstAktiv,
    string BenutzerId);