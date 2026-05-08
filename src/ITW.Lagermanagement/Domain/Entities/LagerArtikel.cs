using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Domain.Entities;

public sealed class LagerArtikel
{
    private LagerArtikel()
    {
        Name             = string.Empty;
        BasisEinheit     = string.Empty;
        ErstelltVonUserId    = string.Empty;
        AktualisiertVonUserId = string.Empty;
    }

    public LagerArtikel(
        Guid id,
        string name,
        ArtikelKategorie kategorie,
        string basisEinheit,
        int? packungsGroesse,
        string? packungsEinheit,
        decimal? verbrauchProPatient,
        bool hatAblaufdatum,
        int mindestbestand,
        string erstelltVonUserId,
        DateTimeOffset erstelltAm)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id darf nicht leer sein.", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name ist erforderlich.", nameof(name));
        if (string.IsNullOrWhiteSpace(basisEinheit))
            throw new ArgumentException("BasisEinheit ist erforderlich.", nameof(basisEinheit));
        if (packungsGroesse.HasValue && packungsGroesse.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(packungsGroesse), "PackungsGröße muss größer als 0 sein.");
        if (verbrauchProPatient.HasValue && verbrauchProPatient.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(verbrauchProPatient), "VerbrauchProPatient muss größer als 0 sein.");
        if (mindestbestand < 0)
            throw new ArgumentOutOfRangeException(nameof(mindestbestand), "Mindestbestand darf nicht negativ sein.");
        if (string.IsNullOrWhiteSpace(erstelltVonUserId))
            throw new ArgumentException("ErstelltVonUserId ist erforderlich.", nameof(erstelltVonUserId));

        Id                   = id;
        Name                 = name.Trim();
        Kategorie            = kategorie;
        BasisEinheit         = basisEinheit.Trim();
        PackungsGroesse      = packungsGroesse;
        PackungsEinheit      = string.IsNullOrWhiteSpace(packungsEinheit) ? null : packungsEinheit.Trim();
        VerbrauchProPatient  = verbrauchProPatient;
        HatAblaufdatum       = hatAblaufdatum;
        Mindestbestand       = mindestbestand;
        IstAktiv             = true;
        ErstelltVonUserId    = erstelltVonUserId.Trim();
        ErstelltAm           = erstelltAm;
        AktualisiertVonUserId = erstelltVonUserId.Trim();
        AktualisiertAm       = erstelltAm;
    }

    public Guid             Id                    { get; private set; }
    public string           Name                  { get; private set; }
    public ArtikelKategorie Kategorie             { get; private set; }
    public string           BasisEinheit          { get; private set; }
    public int?             PackungsGroesse        { get; private set; }
    public string?          PackungsEinheit        { get; private set; }
    public decimal?         VerbrauchProPatient    { get; private set; }
    public bool             HatAblaufdatum         { get; private set; }
    public int              Mindestbestand         { get; private set; }
    public bool             IstAktiv               { get; private set; }
    public DateTimeOffset   ErstelltAm             { get; private set; }
    public string           ErstelltVonUserId      { get; private set; }
    public DateTimeOffset   AktualisiertAm         { get; private set; }
    public string           AktualisiertVonUserId  { get; private set; }

    public void Aktualisiere(
        string name,
        ArtikelKategorie kategorie,
        string basisEinheit,
        int? packungsGroesse,
        string? packungsEinheit,
        decimal? verbrauchProPatient,
        bool hatAblaufdatum,
        int mindestbestand,
        string aktualisiertVonUserId,
        DateTimeOffset aktualisiertAm)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name ist erforderlich.", nameof(name));
        if (string.IsNullOrWhiteSpace(basisEinheit))
            throw new ArgumentException("BasisEinheit ist erforderlich.", nameof(basisEinheit));
        if (packungsGroesse.HasValue && packungsGroesse.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(packungsGroesse), "PackungsGröße muss größer als 0 sein.");
        if (verbrauchProPatient.HasValue && verbrauchProPatient.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(verbrauchProPatient), "VerbrauchProPatient muss größer als 0 sein.");
        if (mindestbestand < 0)
            throw new ArgumentOutOfRangeException(nameof(mindestbestand), "Mindestbestand darf nicht negativ sein.");

        Name                 = name.Trim();
        Kategorie            = kategorie;
        BasisEinheit         = basisEinheit.Trim();
        PackungsGroesse      = packungsGroesse;
        PackungsEinheit      = string.IsNullOrWhiteSpace(packungsEinheit) ? null : packungsEinheit.Trim();
        VerbrauchProPatient  = verbrauchProPatient;
        HatAblaufdatum       = hatAblaufdatum;
        Mindestbestand       = mindestbestand;
        AktualisiertVonUserId = aktualisiertVonUserId.Trim();
        AktualisiertAm       = aktualisiertAm;
    }

    public void Deaktiviere(string aktualisiertVonUserId, DateTimeOffset aktualisiertAm)
    {
        IstAktiv              = false;
        AktualisiertVonUserId = aktualisiertVonUserId.Trim();
        AktualisiertAm        = aktualisiertAm;
    }

    public void Aktiviere(string aktualisiertVonUserId, DateTimeOffset aktualisiertAm)
    {
        IstAktiv              = true;
        AktualisiertVonUserId = aktualisiertVonUserId.Trim();
        AktualisiertAm        = aktualisiertAm;
    }
}
