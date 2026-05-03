using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Domain.Entities;

public sealed class FahrtenbuchEintrag
{
    private FahrtenbuchEintrag()
    {
        FahrerUserId = string.Empty;
        FahrerName = string.Empty;
        BeifahrerName = string.Empty;
        Fahrtzweck = string.Empty;
        ErstelltVonUserId = string.Empty;
    }

    public FahrtenbuchEintrag(
        Guid id,
        Guid fahrzeugId,
        string fahrerUserId,
        string fahrerName,
        string? beifahrerName,
        Guid? routeSessionId,
        Guid? einsatzId,
        FahrtKategorie fahrtKategorie,
        string fahrtzweck,
        DateTimeOffset startzeitUtc,
        DateTimeOffset? endzeitUtc,
        string? startort,
        string? zielort,
        int startKilometerstand,
        int? endKilometerstand,
        decimal? tankmengeLiter,
        int? kilometerstandBeimTanken,
        bool istAutomatischVorbelegt,
        string? bemerkung,
        string erstelltVonUserId,
        DateTimeOffset erstelltAm)
    {
        if (id == Guid.Empty) throw new ArgumentException("Die Fahrtenbuch-ID darf nicht leer sein.", nameof(id));
        if (fahrzeugId == Guid.Empty) throw new ArgumentException("Die Fahrzeug-ID ist erforderlich.", nameof(fahrzeugId));
        if (string.IsNullOrWhiteSpace(fahrerUserId)) throw new ArgumentException("Die Fahrer-UserId ist erforderlich.", nameof(fahrerUserId));
        if (string.IsNullOrWhiteSpace(fahrerName)) throw new ArgumentException("Der Fahrer ist erforderlich.", nameof(fahrerName));
        if (string.IsNullOrWhiteSpace(fahrtzweck)) throw new ArgumentException("Der Fahrtzweck ist erforderlich.", nameof(fahrtzweck));
        if (startKilometerstand < 0) throw new ArgumentOutOfRangeException(nameof(startKilometerstand), "Der Startkilometerstand darf nicht negativ sein.");
        if (endKilometerstand < 0) throw new ArgumentOutOfRangeException(nameof(endKilometerstand), "Der Endkilometerstand darf nicht negativ sein.");
        if (endKilometerstand.HasValue && endKilometerstand.Value < startKilometerstand) throw new ArgumentException("Der Endkilometerstand darf nicht kleiner als der Startkilometerstand sein.", nameof(endKilometerstand));
        if (endzeitUtc.HasValue && endzeitUtc.Value < startzeitUtc) throw new ArgumentException("Die Endzeit darf nicht vor der Startzeit liegen.", nameof(endzeitUtc));
        if (tankmengeLiter.HasValue && tankmengeLiter.Value < 0) throw new ArgumentOutOfRangeException(nameof(tankmengeLiter), "Die Tankmenge darf nicht negativ sein.");
        if (kilometerstandBeimTanken.HasValue && kilometerstandBeimTanken.Value < 0) throw new ArgumentOutOfRangeException(nameof(kilometerstandBeimTanken), "Der Kilometerstand beim Tanken darf nicht negativ sein.");
        if (string.IsNullOrWhiteSpace(erstelltVonUserId)) throw new ArgumentException("Die UserId des Erstellers ist erforderlich.", nameof(erstelltVonUserId));

        Id = id;
        FahrzeugId = fahrzeugId;
        FahrerUserId = fahrerUserId.Trim();
        FahrerName = fahrerName.Trim();
        BeifahrerName = string.IsNullOrWhiteSpace(beifahrerName) ? null : beifahrerName.Trim();
        RouteSessionId = routeSessionId;
        EinsatzId = einsatzId;
        FahrtKategorie = fahrtKategorie;
        Fahrtzweck = fahrtzweck.Trim();
        StartzeitUtc = startzeitUtc;
        EndzeitUtc = endzeitUtc;
        Startort = string.IsNullOrWhiteSpace(startort) ? null : startort.Trim();
        Zielort = string.IsNullOrWhiteSpace(zielort) ? null : zielort.Trim();
        StartKilometerstand = startKilometerstand;
        EndKilometerstand = endKilometerstand;
        GefahreneKilometer = endKilometerstand.HasValue ? endKilometerstand.Value - startKilometerstand : null;
        TankmengeLiter = tankmengeLiter;
        KilometerstandBeimTanken = kilometerstandBeimTanken;
        Status = endzeitUtc.HasValue ? FahrtenbuchStatus.Abgeschlossen : FahrtenbuchStatus.Offen;
        IstAutomatischVorbelegt = istAutomatischVorbelegt;
        Bemerkung = string.IsNullOrWhiteSpace(bemerkung) ? null : bemerkung.Trim();
        ErstelltVonUserId = erstelltVonUserId.Trim();
        ErstelltAm = erstelltAm;
    }

    public Guid Id { get; private set; }

    public Guid FahrzeugId { get; private set; }

    public string FahrerUserId { get; private set; }

    public string FahrerName { get; private set; }

    public string? BeifahrerName { get; private set; }

    public Guid? RouteSessionId { get; private set; }

    public Guid? EinsatzId { get; private set; }

    public FahrtKategorie FahrtKategorie { get; private set; }

    public string Fahrtzweck { get; private set; }

    public DateTimeOffset StartzeitUtc { get; private set; }

    public DateTimeOffset? EndzeitUtc { get; private set; }

    public string? Startort { get; private set; }

    public string? Zielort { get; private set; }

    public int StartKilometerstand { get; private set; }

    public int? EndKilometerstand { get; private set; }

    public int? GefahreneKilometer { get; private set; }

    public decimal? TankmengeLiter { get; private set; }

    public int? KilometerstandBeimTanken { get; private set; }

    public FahrtenbuchStatus Status { get; private set; }

    public bool IstAutomatischVorbelegt { get; private set; }

    public string? Bemerkung { get; private set; }

    public DateTimeOffset ErstelltAm { get; private set; }

    public string ErstelltVonUserId { get; private set; }

    public DateTimeOffset? AktualisiertAm { get; private set; }

    public string? AktualisiertVonUserId { get; private set; }

    public void Abschliessen(
        DateTimeOffset endzeitUtc,
        string? zielort,
        int endKilometerstand,
        string aktualisiertVonUserId,
        DateTimeOffset aktualisiertAm)
    {
        if (endzeitUtc < StartzeitUtc) throw new ArgumentException("Die Endzeit darf nicht vor der Startzeit liegen.", nameof(endzeitUtc));
        if (endKilometerstand < StartKilometerstand) throw new ArgumentException("Der Endkilometerstand darf nicht kleiner als der Startkilometerstand sein.", nameof(endKilometerstand));
        if (string.IsNullOrWhiteSpace(aktualisiertVonUserId)) throw new ArgumentException("Die UserId des Bearbeiters ist erforderlich.", nameof(aktualisiertVonUserId));

        EndzeitUtc = endzeitUtc;
        Zielort = string.IsNullOrWhiteSpace(zielort) ? null : zielort.Trim();
        EndKilometerstand = endKilometerstand;
        GefahreneKilometer = endKilometerstand - StartKilometerstand;
        Status = FahrtenbuchStatus.Abgeschlossen;
        AktualisiertVonUserId = aktualisiertVonUserId.Trim();
        AktualisiertAm = aktualisiertAm;
    }

    public void MarkiereAlsKorrigiert(
        string? bemerkung,
        string aktualisiertVonUserId,
        DateTimeOffset aktualisiertAm)
    {
        if (string.IsNullOrWhiteSpace(aktualisiertVonUserId)) throw new ArgumentException("Die UserId des Bearbeiters ist erforderlich.", nameof(aktualisiertVonUserId));

        Bemerkung = string.IsNullOrWhiteSpace(bemerkung) ? Bemerkung : bemerkung.Trim();
        Status = FahrtenbuchStatus.Korrigiert;
        AktualisiertVonUserId = aktualisiertVonUserId.Trim();
        AktualisiertAm = aktualisiertAm;
    }
}