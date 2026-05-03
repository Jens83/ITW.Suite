namespace ITW.Dienstplan.Domain.Entities;

public sealed class GeplanterDienstTag
{
    private GeplanterDienstTag()
    {
        AktualisiertVonUserId = string.Empty;
    }

    public GeplanterDienstTag(
        Guid id,
        Guid dienstplanPeriodeId,
        DateOnly dienstDatum,
        string? arztUserId,
        string? notfallsanitaeter1UserId,
        string? notfallsanitaeter2UserId,
        string aktualisiertVonUserId,
        DateTimeOffset aktualisiertAm)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID des geplanten Diensttages darf nicht leer sein.", nameof(id));
        }

        if (dienstplanPeriodeId == Guid.Empty)
        {
            throw new ArgumentException("Die ID der Dienstplanperiode darf nicht leer sein.", nameof(dienstplanPeriodeId));
        }

        if (string.IsNullOrWhiteSpace(aktualisiertVonUserId))
        {
            throw new ArgumentException("Die UserId des Bearbeiters ist erforderlich.", nameof(aktualisiertVonUserId));
        }

        Id = id;
        DienstplanPeriodeId = dienstplanPeriodeId;
        DienstDatum = dienstDatum;

        AktualisiereBesatzung(
            arztUserId,
            notfallsanitaeter1UserId,
            notfallsanitaeter2UserId,
            aktualisiertVonUserId,
            aktualisiertAm);
    }

    public Guid Id { get; private set; }

    public Guid DienstplanPeriodeId { get; private set; }

    public DateOnly DienstDatum { get; private set; }

    public string? ArztUserId { get; private set; }

    public string? Notfallsanitaeter1UserId { get; private set; }

    public string? Notfallsanitaeter2UserId { get; private set; }

    public DateTimeOffset AktualisiertAm { get; private set; }

    public string AktualisiertVonUserId { get; private set; } = string.Empty;

    public int AnzahlGeplanteAerzte =>
        string.IsNullOrWhiteSpace(ArztUserId) ? 0 : 1;

    public int AnzahlGeplanteNotfallsanitaeter
    {
        get
        {
            var count = 0;

            if (!string.IsNullOrWhiteSpace(Notfallsanitaeter1UserId))
            {
                count++;
            }

            if (!string.IsNullOrWhiteSpace(Notfallsanitaeter2UserId))
            {
                count++;
            }

            return count;
        }
    }

    public bool HatVollstaendigeBesatzung =>
        AnzahlGeplanteAerzte >= 1 && AnzahlGeplanteNotfallsanitaeter >= 2;

    public void AktualisiereBesatzung(
        string? arztUserId,
        string? notfallsanitaeter1UserId,
        string? notfallsanitaeter2UserId,
        string aktualisiertVonUserId,
        DateTimeOffset aktualisiertAm)
    {
        if (string.IsNullOrWhiteSpace(aktualisiertVonUserId))
        {
            throw new ArgumentException("Die UserId des Bearbeiters ist erforderlich.", nameof(aktualisiertVonUserId));
        }

        var normalisierteArztUserId = NormalisiereUserId(arztUserId);
        var normalisierteNfs1UserId = NormalisiereUserId(notfallsanitaeter1UserId);
        var normalisierteNfs2UserId = NormalisiereUserId(notfallsanitaeter2UserId);

        var doppelteUserIds = new[]
            {
                normalisierteArztUserId,
                normalisierteNfs1UserId,
                normalisierteNfs2UserId
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .GroupBy(x => x!, StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToArray();

        if (doppelteUserIds.Length > 0)
        {
            throw new ArgumentException("Ein Mitarbeiter darf an einem Tag nicht mehrfach derselben Besatzung zugeordnet werden.");
        }

        ArztUserId = normalisierteArztUserId;
        Notfallsanitaeter1UserId = normalisierteNfs1UserId;
        Notfallsanitaeter2UserId = normalisierteNfs2UserId;
        AktualisiertVonUserId = aktualisiertVonUserId.Trim();
        AktualisiertAm = aktualisiertAm;
    }

    private static string? NormalisiereUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return userId.Trim();
    }
}