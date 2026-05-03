using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Domain.Entities;

public sealed class AutoplanLernereignis
{
    private AutoplanLernereignis()
    {
        BearbeitetVonUserId = string.Empty;
    }

    public AutoplanLernereignis(
        Guid id,
        Guid dienstplanPeriodeId,
        DateOnly dienstDatum,
        DienstbesetzungsSlotCode besetzungsSlotCode,
        AutoplanLernereignisTypCode ereignisTypCode,
        string? vorherigeUserId,
        string? neueUserId,
        string? urspruenglichGeplanterUserId,
        string? kontextArztUserId,
        string? kontextNotfallsanitaeter1UserId,
        string? kontextNotfallsanitaeter2UserId,
        DienstausfallGrundCode? ausfallGrundCode,
        string bearbeitetVonUserId,
        DateTimeOffset erfasstAm)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID des Lernereignisses darf nicht leer sein.", nameof(id));
        }

        if (dienstplanPeriodeId == Guid.Empty)
        {
            throw new ArgumentException("Die Dienstplanperiode ist erforderlich.", nameof(dienstplanPeriodeId));
        }

        if (string.IsNullOrWhiteSpace(bearbeitetVonUserId))
        {
            throw new ArgumentException("Die UserId des Bearbeiters ist erforderlich.", nameof(bearbeitetVonUserId));
        }

        var normalisierteVorherigeUserId = NormalisiereUserId(vorherigeUserId);
        var normalisierteNeueUserId = NormalisiereUserId(neueUserId);

        if (string.IsNullOrWhiteSpace(normalisierteVorherigeUserId) &&
            string.IsNullOrWhiteSpace(normalisierteNeueUserId))
        {
            throw new ArgumentException("Mindestens die vorherige oder die neue UserId muss gesetzt sein.");
        }

        if (!string.IsNullOrWhiteSpace(normalisierteVorherigeUserId) &&
            string.Equals(normalisierteVorherigeUserId, normalisierteNeueUserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Vorherige und neue UserId dürfen nicht identisch sein.");
        }

        Id = id;
        DienstplanPeriodeId = dienstplanPeriodeId;
        DienstDatum = dienstDatum;
        BesetzungsSlotCode = besetzungsSlotCode;
        EreignisTypCode = ereignisTypCode;
        VorherigeUserId = normalisierteVorherigeUserId;
        NeueUserId = normalisierteNeueUserId;
        UrspruenglichGeplanterUserId = NormalisiereUserId(urspruenglichGeplanterUserId);
        KontextArztUserId = NormalisiereUserId(kontextArztUserId);
        KontextNotfallsanitaeter1UserId = NormalisiereUserId(kontextNotfallsanitaeter1UserId);
        KontextNotfallsanitaeter2UserId = NormalisiereUserId(kontextNotfallsanitaeter2UserId);
        AusfallGrundCode = ausfallGrundCode;
        BearbeitetVonUserId = bearbeitetVonUserId.Trim();
        ErfasstAm = erfasstAm;
    }

    public Guid Id { get; private set; }

    public Guid DienstplanPeriodeId { get; private set; }

    public DateOnly DienstDatum { get; private set; }

    public DienstbesetzungsSlotCode BesetzungsSlotCode { get; private set; }

    public AutoplanLernereignisTypCode EreignisTypCode { get; private set; }

    public string? VorherigeUserId { get; private set; }

    public string? NeueUserId { get; private set; }

    public string? UrspruenglichGeplanterUserId { get; private set; }

    public string? KontextArztUserId { get; private set; }

    public string? KontextNotfallsanitaeter1UserId { get; private set; }

    public string? KontextNotfallsanitaeter2UserId { get; private set; }

    public DienstausfallGrundCode? AusfallGrundCode { get; private set; }

    public string BearbeitetVonUserId { get; private set; }

    public DateTimeOffset ErfasstAm { get; private set; }

    private static string? NormalisiereUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return userId.Trim();
    }
}