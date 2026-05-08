using ITW.Domain.Personnel.Enums;

namespace ITW.Domain.Personnel.Entities;

public sealed class AllgemeinesMitarbeiterprofil
{
    private AllgemeinesMitarbeiterprofil()
    {
        UserId = string.Empty;
        Vorname = string.Empty;
        Nachname = string.Empty;
        DisplayName = string.Empty;
        Beschaeftigungsart = MitarbeiterBeschaeftigungsart.Unbekannt;
    }

    public AllgemeinesMitarbeiterprofil(
        Guid id,
        string userId,
        DateTimeOffset erstelltAm)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID des allgemeinen Mitarbeiterprofils darf nicht leer sein.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Die UserId darf nicht leer sein.", nameof(userId));
        }

        Id = id;
        UserId = userId.Trim();
        Vorname = string.Empty;
        Nachname = string.Empty;
        DisplayName = string.Empty;
        Beschaeftigungsart = MitarbeiterBeschaeftigungsart.Unbekannt;
        ErstelltAm = erstelltAm;
        AktualisiertAm = erstelltAm;
    }

    public Guid Id { get; private set; }

    public string UserId { get; private set; }

    public string Vorname { get; private set; }

    public string Nachname { get; private set; }

    public string DisplayName { get; private set; }

    public MitarbeiterBeschaeftigungsart Beschaeftigungsart { get; private set; }

    public string? Telefonnummer { get; private set; }

    public string? Strasse { get; private set; }

    public string? Hausnummer { get; private set; }

    public string? Postleitzahl { get; private set; }

    public string? Ort { get; private set; }

    public DateTimeOffset ErstelltAm { get; private set; }

    public DateTimeOffset AktualisiertAm { get; private set; }

    public void AktualisiereStammdaten(
        string vorname,
        string nachname,
        MitarbeiterBeschaeftigungsart beschaeftigungsart,
        string? telefonnummer,
        string? strasse,
        string? hausnummer,
        string? postleitzahl,
        string? ort,
        DateTimeOffset aktualisiertAm)
    {
        if (string.IsNullOrWhiteSpace(vorname))
        {
            throw new ArgumentException("Der Vorname ist erforderlich.", nameof(vorname));
        }

        if (string.IsNullOrWhiteSpace(nachname))
        {
            throw new ArgumentException("Der Nachname ist erforderlich.", nameof(nachname));
        }

        if (!Enum.IsDefined(beschaeftigungsart))
        {
            throw new ArgumentException("Die Beschäftigungsart ist ungültig.", nameof(beschaeftigungsart));
        }

        Vorname = vorname.Trim();
        Nachname = nachname.Trim();
        DisplayName = $"{Vorname} {Nachname}".Trim();
        Beschaeftigungsart = beschaeftigungsart;
        Telefonnummer = NormalisiereOptional(telefonnummer);
        Strasse = NormalisiereOptional(strasse);
        Hausnummer = NormalisiereOptional(hausnummer);
        Postleitzahl = NormalisiereOptional(postleitzahl);
        Ort = NormalisiereOptional(ort);
        AktualisiertAm = aktualisiertAm;
    }

    private static string? NormalisiereOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}