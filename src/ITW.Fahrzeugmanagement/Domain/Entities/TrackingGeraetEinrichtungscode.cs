namespace ITW.Fahrzeugmanagement.Domain.Entities;

public sealed class TrackingGeraetEinrichtungscode
{
    private TrackingGeraetEinrichtungscode()
    {
        TabletName = string.Empty;
        DeviceIdentifier = string.Empty;
        CodeHash = string.Empty;
    }

    public TrackingGeraetEinrichtungscode(
        Guid id,
        string tabletName,
        string deviceIdentifier,
        string codeHash,
        DateTimeOffset gueltigBisUtc,
        DateTimeOffset erstelltAmUtc,
        string? erstelltVonUserId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID darf nicht leer sein.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(tabletName))
        {
            throw new ArgumentException("Der Tablet-Name ist erforderlich.", nameof(tabletName));
        }

        if (string.IsNullOrWhiteSpace(deviceIdentifier))
        {
            throw new ArgumentException("Der Device-Identifier ist erforderlich.", nameof(deviceIdentifier));
        }

        if (string.IsNullOrWhiteSpace(codeHash))
        {
            throw new ArgumentException("Der CodeHash ist erforderlich.", nameof(codeHash));
        }

        if (gueltigBisUtc <= erstelltAmUtc)
        {
            throw new ArgumentException("Der Einrichtungscode muss nach der Erstellung gültig sein.", nameof(gueltigBisUtc));
        }

        Id = id;
        TabletName = tabletName.Trim();
        DeviceIdentifier = deviceIdentifier.Trim();
        CodeHash = codeHash.Trim();
        GueltigBisUtc = gueltigBisUtc.ToUniversalTime();
        ErstelltAmUtc = erstelltAmUtc.ToUniversalTime();
        ErstelltVonUserId = string.IsNullOrWhiteSpace(erstelltVonUserId)
            ? null
            : erstelltVonUserId.Trim();
    }

    public Guid Id { get; private set; }

    public string TabletName { get; private set; }

    public string DeviceIdentifier { get; private set; }

    public string CodeHash { get; private set; }

    public DateTimeOffset GueltigBisUtc { get; private set; }

    public DateTimeOffset ErstelltAmUtc { get; private set; }

    public string? ErstelltVonUserId { get; private set; }

    public DateTimeOffset? EingeloestAmUtc { get; private set; }

    public bool IstEingeloest => EingeloestAmUtc.HasValue;

    public bool IstGueltig(DateTimeOffset jetztUtc)
    {
        return !IstEingeloest && GueltigBisUtc >= jetztUtc.ToUniversalTime();
    }

    public void MarkiereAlsEingeloest(DateTimeOffset eingeloestAmUtc)
    {
        if (IstEingeloest)
        {
            return;
        }

        EingeloestAmUtc = eingeloestAmUtc.ToUniversalTime();
    }
}