using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Domain.Entities;

public sealed class Fahrzeug
{
    private Fahrzeug()
    {
        InterneNummer = string.Empty;
        Kennzeichen = string.Empty;
        Vin = string.Empty;
        Hersteller = string.Empty;
        Modell = string.Empty;
        Fahrzeugtyp = string.Empty;
        ErstelltVonUserId = string.Empty;
        AktualisiertVonUserId = string.Empty;
    }

    public Fahrzeug(
        Guid id,
        string interneNummer,
        string kennzeichen,
        string vin,
        string hersteller,
        string modell,
        string fahrzeugtyp,
        int? baujahr,
        DateOnly? erstzulassung,
        Kraftstoffart kraftstoffart,
        int? leistungKw,
        int kilometerstandAktuell,
        FahrzeugStatus status,
        string? standardStandort,
        string erstelltVonUserId,
        DateTimeOffset erstelltAm)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die Fahrzeug-ID darf nicht leer sein.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(interneNummer))
        {
            throw new ArgumentException("Die interne Nummer ist erforderlich.", nameof(interneNummer));
        }

        if (string.IsNullOrWhiteSpace(kennzeichen))
        {
            throw new ArgumentException("Das Kennzeichen ist erforderlich.", nameof(kennzeichen));
        }

        if (string.IsNullOrWhiteSpace(vin))
        {
            throw new ArgumentException("Die VIN ist erforderlich.", nameof(vin));
        }

        if (string.IsNullOrWhiteSpace(hersteller))
        {
            throw new ArgumentException("Der Hersteller ist erforderlich.", nameof(hersteller));
        }

        if (string.IsNullOrWhiteSpace(modell))
        {
            throw new ArgumentException("Das Modell ist erforderlich.", nameof(modell));
        }

        if (string.IsNullOrWhiteSpace(fahrzeugtyp))
        {
            throw new ArgumentException("Der Fahrzeugtyp ist erforderlich.", nameof(fahrzeugtyp));
        }

        if (baujahr is < 1900 or > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(baujahr), "Das Baujahr ist ungültig.");
        }

        if (leistungKw < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(leistungKw), "Die Leistung darf nicht negativ sein.");
        }

        if (kilometerstandAktuell < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(kilometerstandAktuell), "Der Kilometerstand darf nicht negativ sein.");
        }

        if (string.IsNullOrWhiteSpace(erstelltVonUserId))
        {
            throw new ArgumentException("Die UserId des Erstellers ist erforderlich.", nameof(erstelltVonUserId));
        }

        Id = id;
        InterneNummer = interneNummer.Trim();
        Kennzeichen = kennzeichen.Trim().ToUpperInvariant();
        Vin = vin.Trim().ToUpperInvariant();
        Hersteller = hersteller.Trim();
        Modell = modell.Trim();
        Fahrzeugtyp = fahrzeugtyp.Trim();
        Baujahr = baujahr;
        Erstzulassung = erstzulassung;
        Kraftstoffart = kraftstoffart;
        LeistungKw = leistungKw;
        KilometerstandAktuell = kilometerstandAktuell;
        Status = status;
        StandardStandort = NormalisiereOptionalenText(standardStandort);
        ErstelltVonUserId = erstelltVonUserId.Trim();
        ErstelltAm = erstelltAm;
        AktualisiertVonUserId = erstelltVonUserId.Trim();
        AktualisiertAm = erstelltAm;
    }

    public Guid Id { get; private set; }

    public string InterneNummer { get; private set; }

    public string Kennzeichen { get; private set; }

    public string Vin { get; private set; }

    public string Hersteller { get; private set; }

    public string Modell { get; private set; }

    public string Fahrzeugtyp { get; private set; }

    public int? Baujahr { get; private set; }

    public DateOnly? Erstzulassung { get; private set; }

    public Kraftstoffart Kraftstoffart { get; private set; }

    public int? LeistungKw { get; private set; }

    public int KilometerstandAktuell { get; private set; }

    public FahrzeugStatus Status { get; private set; }

    public string? StandardStandort { get; private set; }

    public DateTimeOffset ErstelltAm { get; private set; }

    public string ErstelltVonUserId { get; private set; }

    public DateTimeOffset AktualisiertAm { get; private set; }

    public string AktualisiertVonUserId { get; private set; }

    public void AktualisiereStammdaten(
        string interneNummer,
        string kennzeichen,
        string vin,
        string hersteller,
        string modell,
        string fahrzeugtyp,
        int? baujahr,
        DateOnly? erstzulassung,
        Kraftstoffart kraftstoffart,
        int? leistungKw,
        FahrzeugStatus status,
        string? standardStandort,
        string aktualisiertVonUserId,
        DateTimeOffset aktualisiertAm)
    {
        if (string.IsNullOrWhiteSpace(interneNummer))
        {
            throw new ArgumentException("Die interne Nummer ist erforderlich.", nameof(interneNummer));
        }

        if (string.IsNullOrWhiteSpace(kennzeichen))
        {
            throw new ArgumentException("Das Kennzeichen ist erforderlich.", nameof(kennzeichen));
        }

        if (string.IsNullOrWhiteSpace(vin))
        {
            throw new ArgumentException("Die VIN ist erforderlich.", nameof(vin));
        }

        if (string.IsNullOrWhiteSpace(hersteller))
        {
            throw new ArgumentException("Der Hersteller ist erforderlich.", nameof(hersteller));
        }

        if (string.IsNullOrWhiteSpace(modell))
        {
            throw new ArgumentException("Das Modell ist erforderlich.", nameof(modell));
        }

        if (string.IsNullOrWhiteSpace(fahrzeugtyp))
        {
            throw new ArgumentException("Der Fahrzeugtyp ist erforderlich.", nameof(fahrzeugtyp));
        }

        if (baujahr is < 1900 or > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(baujahr), "Das Baujahr ist ungültig.");
        }

        if (leistungKw < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(leistungKw), "Die Leistung darf nicht negativ sein.");
        }

        if (string.IsNullOrWhiteSpace(aktualisiertVonUserId))
        {
            throw new ArgumentException("Die UserId des Bearbeiters ist erforderlich.", nameof(aktualisiertVonUserId));
        }

        InterneNummer = interneNummer.Trim();
        Kennzeichen = kennzeichen.Trim().ToUpperInvariant();
        Vin = vin.Trim().ToUpperInvariant();
        Hersteller = hersteller.Trim();
        Modell = modell.Trim();
        Fahrzeugtyp = fahrzeugtyp.Trim();
        Baujahr = baujahr;
        Erstzulassung = erstzulassung;
        Kraftstoffart = kraftstoffart;
        LeistungKw = leistungKw;
        Status = status;
        StandardStandort = NormalisiereOptionalenText(standardStandort);
        AktualisiertVonUserId = aktualisiertVonUserId.Trim();
        AktualisiertAm = aktualisiertAm;
    }

    public void AktualisiereKilometerstand(
        int kilometerstandAktuell,
        string aktualisiertVonUserId,
        DateTimeOffset aktualisiertAm)
    {
        if (kilometerstandAktuell < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(kilometerstandAktuell), "Der Kilometerstand darf nicht negativ sein.");
        }

        if (kilometerstandAktuell < KilometerstandAktuell)
        {
            throw new ArgumentException("Der Kilometerstand darf nicht kleiner als der bisherige Wert sein.", nameof(kilometerstandAktuell));
        }

        if (string.IsNullOrWhiteSpace(aktualisiertVonUserId))
        {
            throw new ArgumentException("Die UserId des Bearbeiters ist erforderlich.", nameof(aktualisiertVonUserId));
        }

        KilometerstandAktuell = kilometerstandAktuell;
        AktualisiertVonUserId = aktualisiertVonUserId.Trim();
        AktualisiertAm = aktualisiertAm;
    }

    private static string? NormalisiereOptionalenText(string? wert)
    {
        return string.IsNullOrWhiteSpace(wert) ? null : wert.Trim();
    }
}