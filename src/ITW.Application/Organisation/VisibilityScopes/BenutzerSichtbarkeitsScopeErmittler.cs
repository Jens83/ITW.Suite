using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Organisation.VisibilityScopes;

public sealed class BenutzerSichtbarkeitsScopeErmittler
{
    public BenutzerSichtbarkeitsScope ErmittleFuerBenutzerlisten(
        OrganisationsbereichCode aufrufenderBereich,
        BereichsrolleCode aufrufendeRolle)
    {
        return (aufrufenderBereich, aufrufendeRolle) switch
        {
            (OrganisationsbereichCode.Intensivtransport, BereichsrolleCode.Wachleiter)
                => BenutzerSichtbarkeitsScope.Erlaubt(OrganisationsbereichCode.Intensivtransport),

            (OrganisationsbereichCode.Verwaltung, BereichsrolleCode.Vorstandsverwaltung)
                => BenutzerSichtbarkeitsScope.Erlaubt(OrganisationsbereichCode.Verwaltung),

            (OrganisationsbereichCode.Vorstand, BereichsrolleCode.Vorstand)
                => BenutzerSichtbarkeitsScope.Erlaubt(OrganisationsbereichCode.Vorstand),

            _ => BenutzerSichtbarkeitsScope.Verweigert(
                "Der Mitarbeiter besitzt keine Berechtigung zum Lesen von Mitarbeiterlisten.")
        };
    }
}