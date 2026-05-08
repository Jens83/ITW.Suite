using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Lagermanagement;

public sealed class LagerUebersichtViewModel
{
    public IReadOnlyList<O2WarnungViewModel>       O2Warnungen       { get; init; } = [];
    public IReadOnlyList<BestandsWarnungViewModel> BestandsWarnungen { get; init; } = [];
    public IReadOnlyList<ChargeWarnungViewModel>   ChargeWarnungen   { get; init; } = [];
    public int AnzahlO2Voll      { get; init; }
    public int AnzahlO2Leer      { get; init; }
    public int AnzahlO2ImFahrzeug { get; init; }

    public bool HatWarnungen =>
        O2Warnungen.Any() || BestandsWarnungen.Any() || ChargeWarnungen.Any();
}

public sealed class O2WarnungViewModel
{
    public Guid   FlascheId      { get; init; }
    public string Bezeichnung    { get; init; } = string.Empty;
    public int    TageImSystem   { get; init; }
    public bool   IstKritisch    { get; init; }
    public int    TageVerbleibend => Math.Max(0, 180 - TageImSystem);
}

public sealed class BestandsWarnungViewModel
{
    public Guid     ArtikelId    { get; init; }
    public string   ArtikelName  { get; init; } = string.Empty;
    public Lagerort Lagerort     { get; init; }
    public int      Menge        { get; init; }
    public int      Mindestbestand { get; init; }
    public string   Einheit      { get; init; } = string.Empty;
}

public sealed class ChargeWarnungViewModel
{
    public Guid    ChargeId     { get; init; }
    public string  ArtikelName  { get; init; } = string.Empty;
    public DateOnly Ablaufdatum { get; init; }
    public int     Menge        { get; init; }
    public string  Einheit      { get; init; } = string.Empty;
    public bool    IstAbgelaufen { get; init; }
}
