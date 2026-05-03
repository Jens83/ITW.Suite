using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Domain.Entities;

public sealed class GeplanterDienstTagAusfall
{
    private GeplanterDienstTagAusfall()
    {
    }

    public GeplanterDienstTagAusfall(
        Guid id,
        Guid dienstplanPeriodeId,
        DateOnly dienstDatum,
        DienstbesetzungsSlotCode besetzungsSlotCode,
        string urspruenglichGeplanterUserId,
        DienstausfallGrundCode ausfallGrundCode,
        string? vertretungsUserId,
        string erfasstVonUserId,
        DateTimeOffset erfasstAm)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID des Ausfalleintrags darf nicht leer sein.", nameof(id));
        }

        if (dienstplanPeriodeId == Guid.Empty)
        {
            throw new ArgumentException("Die ID der Dienstplanperiode darf nicht leer sein.", nameof(dienstplanPeriodeId));
        }

        if (string.IsNullOrWhiteSpace(urspruenglichGeplanterUserId))
        {
            throw new ArgumentException("Die ursprünglich geplante UserId ist erforderlich.", nameof(urspruenglichGeplanterUserId));
        }

        if (string.IsNullOrWhiteSpace(erfasstVonUserId))
        {
            throw new ArgumentException("Die UserId des Bearbeiters ist erforderlich.", nameof(erfasstVonUserId));
        }

        Id = id;
        DienstplanPeriodeId = dienstplanPeriodeId;
        DienstDatum = dienstDatum;
        BesetzungsSlotCode = besetzungsSlotCode;

        Aktualisiere(
            urspruenglichGeplanterUserId,
            ausfallGrundCode,
            vertretungsUserId,
            erfasstVonUserId,
            erfasstAm);
    }

    public Guid Id { get; private set; }

    public Guid DienstplanPeriodeId { get; private set; }

    public DateOnly DienstDatum { get; private set; }

    public DienstbesetzungsSlotCode BesetzungsSlotCode { get; private set; }

    public string UrspruenglichGeplanterUserId { get; private set; } = string.Empty;

    public DienstausfallGrundCode AusfallGrundCode { get; private set; }

    public string? VertretungsUserId { get; private set; }

    public string ErfasstVonUserId { get; private set; } = string.Empty;

    public DateTimeOffset ErfasstAm { get; private set; }

    public void Aktualisiere(
        string urspruenglichGeplanterUserId,
        DienstausfallGrundCode ausfallGrundCode,
        string? vertretungsUserId,
        string erfasstVonUserId,
        DateTimeOffset erfasstAm)
    {
        if (string.IsNullOrWhiteSpace(urspruenglichGeplanterUserId))
        {
            throw new ArgumentException("Die ursprünglich geplante UserId ist erforderlich.", nameof(urspruenglichGeplanterUserId));
        }

        if (string.IsNullOrWhiteSpace(erfasstVonUserId))
        {
            throw new ArgumentException("Die UserId des Bearbeiters ist erforderlich.", nameof(erfasstVonUserId));
        }

        var originalUserId = urspruenglichGeplanterUserId.Trim();
        var vertretung = string.IsNullOrWhiteSpace(vertretungsUserId)
            ? null
            : vertretungsUserId.Trim();

        if (!string.IsNullOrWhiteSpace(vertretung) &&
            string.Equals(originalUserId, vertretung, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Die Vertretung darf nicht identisch mit der ursprünglich geplanten Person sein.");
        }

        UrspruenglichGeplanterUserId = originalUserId;
        AusfallGrundCode = ausfallGrundCode;
        VertretungsUserId = vertretung;
        ErfasstVonUserId = erfasstVonUserId.Trim();
        ErfasstAm = erfasstAm;
    }
}