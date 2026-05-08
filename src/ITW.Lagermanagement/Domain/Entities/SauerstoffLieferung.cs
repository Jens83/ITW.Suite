using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Domain.Entities;

public sealed class SauerstoffLieferung
{
    private SauerstoffLieferung()
    {
        LieferscheinNummer = string.Empty;
        ErfasstVonUserId   = string.Empty;
    }

    public SauerstoffLieferung(
        Guid id,
        string lieferscheinNummer,
        DateOnly lieferdatum,
        string? bemerkung,
        DateTimeOffset erfasstAm,
        string erfasstVonUserId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id darf nicht leer sein.", nameof(id));
        if (string.IsNullOrWhiteSpace(lieferscheinNummer))
            throw new ArgumentException("LieferscheinNummer ist erforderlich.", nameof(lieferscheinNummer));
        if (string.IsNullOrWhiteSpace(erfasstVonUserId))
            throw new ArgumentException("ErfasstVonUserId ist erforderlich.", nameof(erfasstVonUserId));

        Id                 = id;
        LieferscheinNummer = lieferscheinNummer.Trim();
        Lieferdatum        = lieferdatum;
        Bemerkung          = string.IsNullOrWhiteSpace(bemerkung) ? null : bemerkung.Trim();
        ErfasstAm          = erfasstAm;
        ErfasstVonUserId   = erfasstVonUserId.Trim();
    }

    public Guid            Id                 { get; private set; }
    public string          LieferscheinNummer { get; private set; }
    public DateOnly        Lieferdatum        { get; private set; }
    public string?         Bemerkung          { get; private set; }
    public DateTimeOffset  ErfasstAm          { get; private set; }
    public string          ErfasstVonUserId   { get; private set; }
}
