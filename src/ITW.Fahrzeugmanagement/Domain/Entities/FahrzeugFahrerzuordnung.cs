using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Domain.Entities;

public sealed class FahrzeugFahrerzuordnung
{
    private FahrzeugFahrerzuordnung()
    {
        UserId = string.Empty;
        ErstelltVonUserId = string.Empty;
    }

    public FahrzeugFahrerzuordnung(
        Guid id,
        Guid fahrzeugId,
        string userId,
        FahrerzuordnungTyp zuordnungTyp,
        bool istPrimaer,
        DateOnly gueltigVon,
        DateOnly? gueltigBis,
        string? bemerkung,
        string erstelltVonUserId,
        DateTimeOffset erstelltAm)
    {
        if (id == Guid.Empty) throw new ArgumentException("Die Zuordnungs-ID darf nicht leer sein.", nameof(id));
        if (fahrzeugId == Guid.Empty) throw new ArgumentException("Die Fahrzeug-ID ist erforderlich.", nameof(fahrzeugId));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("Die UserId ist erforderlich.", nameof(userId));
        if (gueltigBis.HasValue && gueltigBis.Value < gueltigVon) throw new ArgumentException("GueltigBis darf nicht vor GueltigVon liegen.", nameof(gueltigBis));
        if (string.IsNullOrWhiteSpace(erstelltVonUserId)) throw new ArgumentException("Die UserId des Erstellers ist erforderlich.", nameof(erstelltVonUserId));

        Id = id;
        FahrzeugId = fahrzeugId;
        UserId = userId.Trim();
        ZuordnungTyp = zuordnungTyp;
        IstPrimaer = istPrimaer;
        GueltigVon = gueltigVon;
        GueltigBis = gueltigBis;
        Bemerkung = string.IsNullOrWhiteSpace(bemerkung) ? null : bemerkung.Trim();
        ErstelltVonUserId = erstelltVonUserId.Trim();
        ErstelltAm = erstelltAm;
    }

    public Guid Id { get; private set; }

    public Guid FahrzeugId { get; private set; }

    public string UserId { get; private set; }

    public FahrerzuordnungTyp ZuordnungTyp { get; private set; }

    public bool IstPrimaer { get; private set; }

    public DateOnly GueltigVon { get; private set; }

    public DateOnly? GueltigBis { get; private set; }

    public string? Bemerkung { get; private set; }

    public DateTimeOffset ErstelltAm { get; private set; }

    public string ErstelltVonUserId { get; private set; }
}