namespace ITW.Web.Areas.Intensivtransport.ViewModels.Urlaubsplaner;

public sealed class WachleiterUrlaubsUebersichtViewModel
{
    public IReadOnlyList<WachleiterUrlaubsAntragViewModel> Antraege { get; init; } = [];
}

public sealed class WachleiterUrlaubsAntragViewModel
{
    public Guid    ZeitraumId         { get; init; }
    public string  MitarbeiterName    { get; init; } = string.Empty;
    public string  VonAnzeige         { get; init; } = string.Empty;
    public string  BisAnzeige         { get; init; } = string.Empty;
    public int     Urlaubstage        { get; init; }
    public string? Notiz              { get; init; }
    public bool    HatUeberschneidung { get; init; }
}
