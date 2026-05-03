using System.ComponentModel.DataAnnotations;

namespace ITW.Domain.Personnel.Enums;

public enum MitarbeiterBeschaeftigungsart
{
    [Display(Name = "Unbekannt")]
    Unbekannt = 0,

    [Display(Name = "Festangestellt")]
    Festangestellt = 1,

    [Display(Name = "Freelancer")]
    Freelancer = 2,

    [Display(Name = "Honorarkraft")]
    Honorarkraft = 3
}