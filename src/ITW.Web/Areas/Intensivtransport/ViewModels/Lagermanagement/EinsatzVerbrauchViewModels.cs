using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Lagermanagement;

public sealed class EinsatzVerbrauchIndexViewModel
{
    public IReadOnlyList<EinsatzVerbrauchZeileViewModel> Eintraege { get; init; } = [];
}

public sealed class EinsatzVerbrauchZeileViewModel
{
    public Guid     Id        { get; init; }
    public DateOnly Datum     { get; init; }
    public Lagerort Fahrzeug  { get; init; }
    public int      Patienten { get; init; }
    public string?  Bemerkung { get; init; }

    public string FahrzeugAnzeige => Fahrzeug switch
    {
        Lagerort.Fahrzeug1 => "Fahrzeug 1",
        Lagerort.Fahrzeug2 => "Fahrzeug 2",
        _                  => Fahrzeug.ToString()
    };
}

public sealed class EinsatzVerbrauchFormViewModel
{
    public DateOnly  Datum     { get; set; }
    public Lagerort  Fahrzeug  { get; set; } = Lagerort.Fahrzeug1;
    public int       Patienten { get; set; }
    public string?   Bemerkung { get; set; }
    public IReadOnlyList<ArtikelAuswahlViewModel>    VerfuegbareArtikel { get; set; } = [];
    public List<EinsatzVerbrauchPositionFormViewModel> Positionen        { get; set; } = [];
}

public sealed class EinsatzVerbrauchPositionFormViewModel
{
    public Guid ArtikelId { get; set; }
    public int  Menge     { get; set; }
}

public sealed class ArtikelAuswahlViewModel
{
    public Guid     Id                  { get; init; }
    public string   Name                { get; init; } = string.Empty;
    public string   Einheit             { get; init; } = string.Empty;
    public decimal? VerbrauchProPatient { get; init; }
}
