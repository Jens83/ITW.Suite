using ITW.Domain.Organisation.Enums;

namespace ITW.Application.Organisation.Contracts;

internal static class OrganisationEnumMapper
{
    public static Organisationsbereich ToDomain(this OrganisationsbereichCode value)
    {
        return value switch
        {
            OrganisationsbereichCode.Intensivtransport => Organisationsbereich.Intensivtransport,
            OrganisationsbereichCode.Verwaltung => Organisationsbereich.Verwaltung,
            OrganisationsbereichCode.Vorstand => Organisationsbereich.Geschaeftsfuehrung,
            _ => Organisationsbereich.Unbekannt
        };
    }

    public static Bereichsrolle ToDomain(this BereichsrolleCode value)
    {
        return value switch
        {
            BereichsrolleCode.Mitarbeiter => Bereichsrolle.ItwMitarbeiter,
            BereichsrolleCode.Wachleiter => Bereichsrolle.Wachleiter,
            BereichsrolleCode.Verwaltungsmitarbeiter => Bereichsrolle.Verwaltungsmitarbeiter,
            BereichsrolleCode.Vorstandsverwaltung => Bereichsrolle.GeschaeftsfuehrerVerwaltung,
            BereichsrolleCode.Vorstand => Bereichsrolle.Geschaeftsfuehrung,
            _ => Bereichsrolle.Unbekannt
        };
    }

    public static Fuehrungsverantwortung ToDomain(this FuehrungsverantwortungCode value)
    {
        return value switch
        {
            FuehrungsverantwortungCode.Keine => Fuehrungsverantwortung.Keine,
            FuehrungsverantwortungCode.OperativeLeitung => Fuehrungsverantwortung.OperativeLeitung,
            FuehrungsverantwortungCode.Bereichsleitung => Fuehrungsverantwortung.Bereichsleitung,
            FuehrungsverantwortungCode.UebergeordneteLeitung => Fuehrungsverantwortung.UebergeordneteLeitung,
            _ => Fuehrungsverantwortung.Keine
        };
    }

    public static Modul ToDomain(this ModulCode value)
    {
        return value switch
        {
            ModulCode.Dienstplan => Modul.Dienstplan,
            ModulCode.Einsatzverwaltung => Modul.Einsatzverwaltung,
            ModulCode.Personal => Modul.Personal,
            ModulCode.Abrechnung => Modul.Abrechnung,
            ModulCode.Buchhaltung => Modul.Buchhaltung,
            ModulCode.Lagerlogistik => Modul.Lagerlogistik,
            ModulCode.Sauerstofflager => Modul.Sauerstofflager,
            ModulCode.Fahrzeugmanagement => Modul.Fahrzeugmanagement,
            ModulCode.TabletApp => Modul.TabletApp,
            _ => Modul.Unbekannt
        };
    }

    public static OrganisationsbereichCode ToApplication(this Organisationsbereich value)
    {
        return value switch
        {
            Organisationsbereich.Intensivtransport => OrganisationsbereichCode.Intensivtransport,
            Organisationsbereich.Verwaltung => OrganisationsbereichCode.Verwaltung,
            Organisationsbereich.Geschaeftsfuehrung => OrganisationsbereichCode.Vorstand,
            _ => OrganisationsbereichCode.Unbekannt
        };
    }

    public static BereichsrolleCode ToApplication(this Bereichsrolle value)
    {
        return value switch
        {
            Bereichsrolle.ItwMitarbeiter => BereichsrolleCode.Mitarbeiter,
            Bereichsrolle.Wachleiter => BereichsrolleCode.Wachleiter,
            Bereichsrolle.Verwaltungsmitarbeiter => BereichsrolleCode.Verwaltungsmitarbeiter,
            Bereichsrolle.GeschaeftsfuehrerVerwaltung => BereichsrolleCode.Vorstandsverwaltung,
            Bereichsrolle.Geschaeftsfuehrung => BereichsrolleCode.Vorstand,
            _ => BereichsrolleCode.Unbekannt
        };
    }

    public static FuehrungsverantwortungCode ToApplication(this Fuehrungsverantwortung value)
    {
        return value switch
        {
            Fuehrungsverantwortung.Keine => FuehrungsverantwortungCode.Keine,
            Fuehrungsverantwortung.OperativeLeitung => FuehrungsverantwortungCode.OperativeLeitung,
            Fuehrungsverantwortung.Bereichsleitung => FuehrungsverantwortungCode.Bereichsleitung,
            Fuehrungsverantwortung.UebergeordneteLeitung => FuehrungsverantwortungCode.UebergeordneteLeitung,
            _ => FuehrungsverantwortungCode.Keine
        };
    }

    public static ModulCode ToApplication(this Modul value)
    {
        return value switch
        {
            Modul.Dienstplan => ModulCode.Dienstplan,
            Modul.Einsatzverwaltung => ModulCode.Einsatzverwaltung,
            Modul.Personal => ModulCode.Personal,
            Modul.Abrechnung => ModulCode.Abrechnung,
            Modul.Buchhaltung => ModulCode.Buchhaltung,
            Modul.Lagerlogistik => ModulCode.Lagerlogistik,
            Modul.Sauerstofflager => ModulCode.Sauerstofflager,
            Modul.Fahrzeugmanagement => ModulCode.Fahrzeugmanagement,
            Modul.TabletApp => ModulCode.TabletApp,
            _ => ModulCode.Unbekannt
        };
    }
}