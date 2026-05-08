using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Domain.Entities;

/// <summary>
/// Charge eines Artikels mit Ablaufdatum (ausschließlich für Medikamente).
/// Jede Einlieferung eines Medikaments erzeugt eine eigene Charge.
/// </summary>
public sealed class ArtikelCharge
{
    private ArtikelCharge()
    {
        ChargeNummer      = string.Empty;
        EingebuchtVonUserId = string.Empty;
    }

    public ArtikelCharge(
        Guid id,
        Guid artikelId,
        Lagerort lagerort,
        int menge,
        DateOnly ablaufdatum,
        string? chargeNummer,
        DateOnly eingebuchtAm,
        string eingebuchtVonUserId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id darf nicht leer sein.", nameof(id));
        if (artikelId == Guid.Empty)
            throw new ArgumentException("ArtikelId darf nicht leer sein.", nameof(artikelId));
        if (menge <= 0)
            throw new ArgumentOutOfRangeException(nameof(menge), "Menge muss größer als 0 sein.");
        if (string.IsNullOrWhiteSpace(eingebuchtVonUserId))
            throw new ArgumentException("EingebuchtVonUserId ist erforderlich.", nameof(eingebuchtVonUserId));

        Id                  = id;
        ArtikelId           = artikelId;
        Lagerort            = lagerort;
        Menge               = menge;
        Ablaufdatum         = ablaufdatum;
        ChargeNummer        = string.IsNullOrWhiteSpace(chargeNummer) ? string.Empty : chargeNummer.Trim();
        EingebuchtAm        = eingebuchtAm;
        EingebuchtVonUserId = eingebuchtVonUserId.Trim();
    }

    public Guid      Id                  { get; private set; }
    public Guid      ArtikelId           { get; private set; }
    public Lagerort  Lagerort            { get; private set; }
    public int       Menge               { get; private set; }
    public DateOnly  Ablaufdatum         { get; private set; }
    public string    ChargeNummer        { get; private set; }
    public DateOnly  EingebuchtAm        { get; private set; }
    public string    EingebuchtVonUserId { get; private set; }
    public bool      IstAusgebucht       { get; private set; }
    public DateOnly? AusgebuchtAm        { get; private set; }
    public string?   AusgebuchtVonUserId { get; private set; }

    public bool IstAbgelaufen(DateOnly heute) => Ablaufdatum < heute;

    public bool LäuftBaldAb(DateOnly heute, int vorwarnungTage = 30)
        => !IstAusgebucht && Ablaufdatum >= heute && Ablaufdatum <= heute.AddDays(vorwarnungTage);

    public void Ausbuchen(string ausgebuchtVonUserId, DateOnly ausgebuchtAm)
    {
        if (IstAusgebucht)
            throw new InvalidOperationException("Charge wurde bereits ausgebucht.");
        if (string.IsNullOrWhiteSpace(ausgebuchtVonUserId))
            throw new ArgumentException("AusgebuchtVonUserId ist erforderlich.", nameof(ausgebuchtVonUserId));

        IstAusgebucht       = true;
        AusgebuchtAm        = ausgebuchtAm;
        AusgebuchtVonUserId = ausgebuchtVonUserId.Trim();
        Menge               = 0;
    }

    public void MengeAnpassen(int neueMenge)
    {
        if (IstAusgebucht)
            throw new InvalidOperationException("Ausgebuchte Chargen können nicht mehr angepasst werden.");
        if (neueMenge < 0)
            throw new ArgumentOutOfRangeException(nameof(neueMenge), "Menge darf nicht negativ sein.");

        Menge = neueMenge;
    }
}
