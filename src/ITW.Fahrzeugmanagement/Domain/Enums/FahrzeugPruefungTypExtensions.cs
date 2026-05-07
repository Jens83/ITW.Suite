namespace ITW.Fahrzeugmanagement.Domain.Enums;

public static class FahrzeugPruefungTypExtensions
{
    public static string ToDisplayText(this FahrzeugPruefungTyp typ) => typ switch
    {
        FahrzeugPruefungTyp.HuAu                                => "HU/AU",
        FahrzeugPruefungTyp.SicherheitspruefungElektrischeAnlage => "Sicherheitsprüfung elektrische Anlage",
        FahrzeugPruefungTyp.SicherheitspruefungSauerstoffanlage  => "Sicherheitsprüfung Sauerstoffanlage",
        FahrzeugPruefungTyp.SicherheitspruefungAufbau            => "Sicherheitsprüfung Aufbau",
        FahrzeugPruefungTyp.Service                             => "Service",
        _                                                       => "Prüfung"
    };
}
