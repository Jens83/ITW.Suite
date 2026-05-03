using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Dienstplan;

public sealed class MonatsauswertungViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public bool IsSuccess { get; set; } = true;

    public string? ErrorMessage { get; set; }

    public Guid? AusgewaehltePeriodeId { get; set; }

    public string AusgewaehltePeriodeBezeichnung { get; set; } = string.Empty;

    public IReadOnlyList<SelectListItem> PeriodenOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<MonatsauswertungMitarbeiterViewModel> Mitarbeiter { get; set; }
        = Array.Empty<MonatsauswertungMitarbeiterViewModel>();

    public int SummeGeplanteDienste { get; set; }

    public int SummeKrankheitstage { get; set; }

    public int SummeUrlaubstage { get; set; }

    public int SummeVertretungen { get; set; }

    public int SummeGefahreneDienste { get; set; }

    public int SummeGesamt { get; set; }
}

public sealed class MonatsauswertungMitarbeiterViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string AnzeigeName { get; set; } = string.Empty;

    public string Hauptqualifikation { get; set; } = string.Empty;

    public int GeplanteDienste { get; set; }

    public int Krankheitstage { get; set; }

    public int Urlaubstage { get; set; }

    public int Vertretungen { get; set; }

    public int GefahreneDienste { get; set; }

    public int Gesamt { get; set; }

    public int Jahresurlaubsanspruch { get; set; }

    public int GenommeneUrlaubstageImJahr { get; set; }

    public int Resturlaubstage { get; set; }
}