// Datei: src/ITW.Web/UI/OrganisationDisplayText.cs
using ITW.Application.Organisation.Contracts;

namespace ITW.Web.UI;

public static class OrganisationDisplayText
{
    public static string Fuer(OrganisationsbereichCode wert)
    {
        return wert switch
        {
            OrganisationsbereichCode.Intensivtransport => "Intensivtransport",
            OrganisationsbereichCode.Verwaltung => "Verwaltung",
            OrganisationsbereichCode.Vorstand => "Geschäftsführung",
            _ => "Unbekannt"
        };
    }

    public static string Fuer(BereichsrolleCode wert)
    {
        return wert switch
        {
            BereichsrolleCode.Mitarbeiter => "Mitarbeiter",
            BereichsrolleCode.Wachleiter => "Wachleiter",
            BereichsrolleCode.Verwaltungsmitarbeiter => "Verwaltungsmitarbeiter",
            BereichsrolleCode.Vorstandsverwaltung => "Geschäftsführer Verwaltung",
            BereichsrolleCode.Vorstand => "Geschäftsführung",
            _ => "Unbekannt"
        };
    }

    public static string Fuer(FuehrungsverantwortungCode wert)
    {
        return wert switch
        {
            FuehrungsverantwortungCode.Keine => "Keine",
            FuehrungsverantwortungCode.OperativeLeitung => "Operative Leitung",
            FuehrungsverantwortungCode.Bereichsleitung => "Bereichsleitung",
            FuehrungsverantwortungCode.UebergeordneteLeitung => "Übergeordnete Leitung",
            _ => "Keine"
        };
    }
}