using System.ComponentModel.DataAnnotations;
using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Lagermanagement;

public sealed class LagerArtikelIndexViewModel
{
    public IReadOnlyList<ArtikelZeileViewModel> Zeilen { get; init; } = [];
}

public sealed class ArtikelZeileViewModel
{
    public Guid             Id                  { get; init; }
    public string           Name                { get; init; } = string.Empty;
    public ArtikelKategorie Kategorie           { get; init; }
    public string           Einheit             { get; init; } = string.Empty;
    public int?             PackungsGroesse     { get; init; }
    public string?          PackungsEinheit     { get; init; }
    public decimal?         VerbrauchProPatient { get; init; }
    public bool             HatAblaufdatum      { get; init; }
    public int              Mindestbestand      { get; init; }
    public bool             IstAktiv            { get; init; }
    public int              MengeDepot          { get; init; }
    public int              MengeFahrzeug1      { get; init; }
    public int              MengeFahrzeug2      { get; init; }
    public int              Gesamtbestand       { get; init; }
    public bool             IstUnterMindest     { get; init; }

    public string PackungsAnzeige =>
        PackungsGroesse.HasValue && !string.IsNullOrEmpty(PackungsEinheit)
            ? $"1 {PackungsEinheit} = {PackungsGroesse} {Einheit}"
            : string.Empty;
}

public sealed class ArtikelFormViewModel
{
    public Guid             Id                  { get; init; }

    [Required(ErrorMessage = "Name ist erforderlich.")]
    [MaxLength(200)]
    public string           Name                { get; set; } = string.Empty;

    public ArtikelKategorie Kategorie           { get; set; }

    [Required(ErrorMessage = "Basiseinheit ist erforderlich.")]
    [MaxLength(50)]
    public string           BasisEinheit        { get; set; } = string.Empty;

    [Range(1, 10000)]
    public int?             PackungsGroesse     { get; set; }

    [MaxLength(50)]
    public string?          PackungsEinheit     { get; set; }

    [Range(0.01, 100)]
    public decimal?         VerbrauchProPatient { get; set; }

    public bool             HatAblaufdatum      { get; set; }

    [Range(0, 100000)]
    public int              Mindestbestand      { get; set; }
}
