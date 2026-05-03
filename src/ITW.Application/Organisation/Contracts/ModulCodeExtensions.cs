namespace ITW.Application.Organisation.Contracts;

public static class ModulCodeExtensions
{
    public static string GetAnzeigeName(this ModulCode modul)
    {
        return modul switch
        {
            ModulCode.Dienstplan => "Dienstplan",
            ModulCode.Einsatzverwaltung => "Einsatzverwaltung",
            ModulCode.Lagerlogistik => "Lagerlogistik",
            ModulCode.Sauerstofflager => "Sauerstofflager",
            ModulCode.Abrechnung => "Abrechnung",
            ModulCode.Personal => "Personal",
            ModulCode.Fahrzeugmanagement => "Fahrzeugmanagement",
            ModulCode.Buchhaltung => "Buchhaltung",
            ModulCode.TabletApp => "Tablet-App",
            _ => "Unbekannt"
        };
    }
}