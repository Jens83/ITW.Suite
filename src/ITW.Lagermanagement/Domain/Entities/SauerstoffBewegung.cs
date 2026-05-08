using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Domain.Entities;

public sealed class SauerstoffBewegung
{
    private SauerstoffBewegung()
    {
        ErstelltVonUserId = string.Empty;
    }

    public SauerstoffBewegung(
        Guid id,
        Guid flascheId,
        SauerstoffBewegungsTyp typ,
        Guid? vonFahrzeugId,
        Guid? nachFahrzeugId,
        DateTimeOffset datum,
        string erstelltVonUserId,
        string? bemerkung = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id darf nicht leer sein.", nameof(id));
        if (flascheId == Guid.Empty)
            throw new ArgumentException("FlascheId darf nicht leer sein.", nameof(flascheId));
        if (string.IsNullOrWhiteSpace(erstelltVonUserId))
            throw new ArgumentException("ErstelltVonUserId ist erforderlich.", nameof(erstelltVonUserId));

        Id                = id;
        FlascheId         = flascheId;
        Typ               = typ;
        VonFahrzeugId     = vonFahrzeugId;
        NachFahrzeugId    = nachFahrzeugId;
        Datum             = datum;
        ErstelltVonUserId = erstelltVonUserId.Trim();
        Bemerkung         = string.IsNullOrWhiteSpace(bemerkung) ? null : bemerkung.Trim();
    }

    public Guid                   Id                { get; private set; }
    public Guid                   FlascheId         { get; private set; }
    public SauerstoffBewegungsTyp Typ               { get; private set; }
    public Guid?                  VonFahrzeugId     { get; private set; }
    public Guid?                  NachFahrzeugId    { get; private set; }
    public DateTimeOffset         Datum             { get; private set; }
    public string                 ErstelltVonUserId { get; private set; }
    public string?                Bemerkung         { get; private set; }
}
