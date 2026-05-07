using ITW.Domain.Personnel.Enums;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Urlaubsplaner;

public sealed class WachleiterJahresuebersichtViewModel
{
    public int  Jahr              { get; init; }
    public int  AnzahlAusstehend  { get; init; }
    public IReadOnlyList<int> JahrOptionen { get; init; } = [];

    public IReadOnlyList<WachleiterGanttReiheViewModel>        Mitarbeiter       { get; init; } = [];
    public IReadOnlyList<WachleiterUeberschneidungViewModel>   Ueberschneidungen { get; init; } = [];

    // Monatstrennlinien für den Gantt-Header (0–100 %)
    public IReadOnlyList<GanttMonatsmarkerViewModel> Monatsmarker { get; init; } = [];
}

public sealed class WachleiterGanttReiheViewModel
{
    public string  UserId      { get; init; } = string.Empty;
    public string  AnzeigeName { get; init; } = string.Empty;
    public int     GesamtTage  { get; init; }
    public IReadOnlyList<WachleiterGanttBalkenViewModel> Balken { get; init; } = [];
}

public sealed class WachleiterGanttBalkenViewModel
{
    public Guid                  ZeitraumId         { get; init; }
    public DateOnly              Von                 { get; init; }
    public DateOnly              Bis                 { get; init; }
    public int                   Tage                { get; init; }
    public UrlaubszeitraumStatus Status              { get; init; }
    public bool                  HatUeberschneidung  { get; init; }
    public double                LeftPercent         { get; init; }
    public double                WidthPercent        { get; init; }

    public string TooltipText =>
        $"{Von:dd.MM.} – {Bis:dd.MM.yyyy} ({Tage} Tage{(HatUeberschneidung ? " – Überschneidung!" : "")})";
}

public sealed class WachleiterUeberschneidungViewModel
{
    public DateOnly     Von              { get; init; }
    public DateOnly     Bis              { get; init; }
    public List<string> MitarbeiterNamen { get; init; } = [];
    public double       LeftPercent      { get; init; }
    public double       WidthPercent     { get; init; }

    public string AnzeigeText =>
        string.Join(" + ", MitarbeiterNamen);

    public string DatumText =>
        Von == Bis
            ? Von.ToString("dd.MM.yyyy")
            : $"{Von:dd.MM.} – {Bis:dd.MM.yyyy}";
}

public sealed class GanttMonatsmarkerViewModel
{
    public string Name        { get; init; } = string.Empty;
    public double LeftPercent { get; init; }
}
