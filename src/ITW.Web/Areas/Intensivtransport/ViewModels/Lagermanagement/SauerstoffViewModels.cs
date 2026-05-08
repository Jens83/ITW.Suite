using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Lagermanagement;

public sealed class SauerstoffIndexViewModel
{
    public IReadOnlyList<SauerstoffFlascheViewModel>    Flaschen             { get; init; } = [];
    public IReadOnlyList<FahrzeugKurzinfo>              AktiveFahrzeuge      { get; init; } = [];
    public IReadOnlyList<SauerstoffLieferungKurzinfo>   OffeneLieferungen    { get; init; } = [];
    public int                                          AnzahlVollDepot      { get; init; }
    public int                                          AnzahlLeerDepot      { get; init; }
    public IReadOnlyList<FahrzeugBestandInfo>           FahrzeugBestaende    { get; init; } = [];
}

public sealed class SauerstoffFlascheViewModel
{
    public Guid                      Id                { get; init; }
    public Guid                      LieferungId       { get; init; }
    public Guid?                     FahrzeugId        { get; init; }
    public SauerstoffFlaschenStatus  Status            { get; init; }
    public SauerstoffFlaschenGroesse Groesse           { get; init; }
    public string?                   FlaschenNummer    { get; init; }
    public int                       TageImSystem      { get; init; }
    public bool                      HatRisiko         { get; init; }
    public bool                      IstKritisch       { get; init; }
    public int                       TageVerbleibend   { get; init; }
    public string                    LieferungInfo     { get; init; } = string.Empty;

    public string BezeichnungAnzeige =>
        string.IsNullOrEmpty(FlaschenNummer)
            ? $"{GroesseAnzeige} Flasche"
            : $"{GroesseAnzeige} · Nr. {FlaschenNummer}";

    public string GroesseAnzeige => Groesse switch
    {
        SauerstoffFlaschenGroesse.L2  => "2 L",
        SauerstoffFlaschenGroesse.L5  => "5 L",
        SauerstoffFlaschenGroesse.L10 => "10 L",
        _                             => $"{(int)Groesse} L"
    };
}

public sealed class SauerstoffScanViewModel
{
    public IReadOnlyList<FahrzeugKurzinfo> AktiveFahrzeuge { get; init; } = [];
}

public sealed class SauerstoffScanResultViewModel
{
    public bool   Gefunden       { get; init; }
    public Guid   FlascheId      { get; init; }
    public string Bezeichnung    { get; init; } = string.Empty;
    public string StatusAnzeige  { get; init; } = string.Empty;
    public string StandortAnzeige { get; init; } = string.Empty;
    public int    TageImSystem   { get; init; }
    public bool   IstKritisch    { get; init; }
    public bool   KannNachFahrzeug { get; init; }
    public bool   KannLeerZurueck  { get; init; }
    public string? FehlerMeldung   { get; init; }
    public IReadOnlyList<FahrzeugKurzinfo> AktiveFahrzeuge { get; init; } = [];
}

public sealed class SauerstoffLieferungenViewModel
{
    public IReadOnlyList<SauerstoffLieferungListItem> Lieferungen { get; init; } = [];
}

public sealed class SauerstoffLieferungListItem
{
    public Guid     Id                 { get; init; }
    public string   LieferscheinNummer { get; init; } = string.Empty;
    public DateOnly Lieferdatum        { get; init; }
    public string?  Bemerkung          { get; init; }
    public int      AnzahlFlaschen     { get; init; }
}

public sealed class SauerstoffLieferungDetailViewModel
{
    public Guid     Id                 { get; init; }
    public string   LieferscheinNummer { get; init; } = string.Empty;
    public DateOnly Lieferdatum        { get; init; }
    public string?  Bemerkung          { get; init; }
    public IReadOnlyList<SauerstoffFlascheViewModel> Flaschen { get; init; } = [];
}

public sealed class SauerstoffAbgabeViewModel
{
    public IReadOnlyList<Guid> LeereFlaschenIds { get; set; } = [];
    public string? Bemerkung { get; set; }
}

// Records
public sealed record FahrzeugKurzinfo(Guid Id, string Kennzeichen);
public sealed record SauerstoffLieferungKurzinfo(Guid Id, string LieferscheinNummer, DateOnly Datum);
public sealed record FahrzeugBestandInfo(Guid FahrzeugId, string Kennzeichen, int AnzahlFlaschen);
