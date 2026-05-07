using ITW.Domain.Personnel.Enums;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Urlaubsplaner;

public sealed class MitarbeiterUrlaubViewModel
{
    public string Titel       { get; init; } = "Mein Urlaub";
    public string Beschreibung { get; init; } = "Urlaubsanträge stellen und den Status deiner Anträge einsehen.";
    public bool   IsSuccess    { get; init; } = true;
    public string? ErrorMessage { get; init; }

    public int  AusgewaehltesJahr   { get; init; }
    public int  Anspruchstage       { get; init; }
    public int  Uebertragstage      { get; init; }
    public int  GenommeneUrlaubstage { get; init; }
    public int  AusstehendeTage     { get; init; }
    public int  Resturlaubstage     { get; init; }
    public bool AnspruchIstStandardwert { get; init; }
    public bool KannUrlaub { get; init; }

    public IReadOnlyList<MitarbeiterUrlaubAntragViewModel> Antraege { get; init; } = [];
}

public sealed class MitarbeiterUrlaubAntragViewModel
{
    public Guid    Id          { get; init; }
    public string  VonAnzeige  { get; init; } = string.Empty;
    public string  BisAnzeige  { get; init; } = string.Empty;
    public int     Urlaubstage { get; init; }
    public string? Notiz       { get; init; }

    public UrlaubszeitraumStatus Status      { get; init; }
    public string?               Begruendung { get; init; }
    public string?               Loesung     { get; init; }

    public string StatusText => Status switch
    {
        UrlaubszeitraumStatus.Genehmigt  => "Genehmigt",
        UrlaubszeitraumStatus.Ausstehend => "Ausstehend",
        UrlaubszeitraumStatus.Abgelehnt  => "Abgelehnt",
        _                                => "Unbekannt"
    };

    public string StatusCssClass => Status switch
    {
        UrlaubszeitraumStatus.Genehmigt  => "app-pill--success",
        UrlaubszeitraumStatus.Ausstehend => "app-pill--warning",
        UrlaubszeitraumStatus.Abgelehnt  => "app-pill--danger",
        _                                => "app-pill--neutral"
    };
}
