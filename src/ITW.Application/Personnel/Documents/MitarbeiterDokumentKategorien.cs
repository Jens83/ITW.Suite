// Datei: src/ITW.Application/Personnel/Documents/MitarbeiterDokumentKategorien.cs
namespace ITW.Application.Personnel.Documents;

public static class MitarbeiterDokumentKategorien
{
    public const string Urkunde = "Urkunde";
    public const string DiviNachweis = "DIVI-Nachweis";
    public const string Fuehrerschein = "Führerschein";
    public const string Fortbildungsnachweis = "Fortbildungsnachweis";
    public const string Personalausweis = "Personalausweis";    
    public const string Sonstiges = "Sonstiges";

    public static readonly IReadOnlyList<string> Alle =
    [
        Urkunde,
        DiviNachweis,
        Fuehrerschein,
        Fortbildungsnachweis,
        Personalausweis,        
        Sonstiges
    ];

    public static bool IstErlaubt(string? kategorie)
    {
        if (string.IsNullOrWhiteSpace(kategorie))
        {
            return false;
        }

        return Alle.Any(x => string.Equals(x, kategorie.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}