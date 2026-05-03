using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Domain.Entities;

public sealed class FahrzeugPruefung
{
    private FahrzeugPruefung()
    {
        Bemerkung = null;
        ErstelltVonUserId = string.Empty;
    }

    public FahrzeugPruefung(
        Guid id,
        Guid fahrzeugId,
        FahrzeugPruefungTyp typ,
        DateOnly faelligAm,
        DateOnly? letzteErledigungAm,
        string? bemerkung,
        string erstelltVonUserId,
        DateTimeOffset erstelltAm)
    {
        if (id == Guid.Empty) throw new ArgumentException("Die Prüfungs-ID darf nicht leer sein.", nameof(id));
        if (fahrzeugId == Guid.Empty) throw new ArgumentException("Die Fahrzeug-ID ist erforderlich.", nameof(fahrzeugId));
        if (typ == FahrzeugPruefungTyp.Unbekannt) throw new ArgumentException("Der Prüfungstyp ist erforderlich.", nameof(typ));
        if (string.IsNullOrWhiteSpace(erstelltVonUserId)) throw new ArgumentException("Die UserId des Erstellers ist erforderlich.", nameof(erstelltVonUserId));

        Id = id;
        FahrzeugId = fahrzeugId;
        Typ = typ;
        FaelligAm = faelligAm;
        LetzteErledigungAm = letzteErledigungAm;
        Bemerkung = string.IsNullOrWhiteSpace(bemerkung) ? null : bemerkung.Trim();
        ErstelltVonUserId = erstelltVonUserId.Trim();
        ErstelltAm = erstelltAm;
        AktualisiertVonUserId = erstelltVonUserId.Trim();
        AktualisiertAm = erstelltAm;
    }

    public Guid Id { get; private set; }

    public Guid FahrzeugId { get; private set; }

    public FahrzeugPruefungTyp Typ { get; private set; }

    public DateOnly FaelligAm { get; private set; }

    public DateOnly? LetzteErledigungAm { get; private set; }

    public string? Bemerkung { get; private set; }

    public DateTimeOffset ErstelltAm { get; private set; }

    public string ErstelltVonUserId { get; private set; }

    public DateTimeOffset AktualisiertAm { get; private set; }

    public string AktualisiertVonUserId { get; private set; } = string.Empty;

    public void Aktualisiere(
        DateOnly faelligAm,
        DateOnly? letzteErledigungAm,
        string? bemerkung,
        string aktualisiertVonUserId,
        DateTimeOffset aktualisiertAm)
    {
        if (string.IsNullOrWhiteSpace(aktualisiertVonUserId))
        {
            throw new ArgumentException("Die UserId des Bearbeiters ist erforderlich.", nameof(aktualisiertVonUserId));
        }

        FaelligAm = faelligAm;
        LetzteErledigungAm = letzteErledigungAm;
        Bemerkung = string.IsNullOrWhiteSpace(bemerkung) ? null : bemerkung.Trim();
        AktualisiertVonUserId = aktualisiertVonUserId.Trim();
        AktualisiertAm = aktualisiertAm;
    }
}