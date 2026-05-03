namespace ITW.Fahrzeugmanagement.Domain.Entities;

public sealed class FahrzeugTrackingGeraet
{
    private FahrzeugTrackingGeraet()
    {
        DeviceIdentifier = string.Empty;
        ApiKeyHash = string.Empty;
    }

    public FahrzeugTrackingGeraet(
        Guid id,
        string deviceIdentifier,
        string apiKeyHash,
        bool istAktiv)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die Geräte-ID darf nicht leer sein.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(deviceIdentifier))
        {
            throw new ArgumentException("Der Device-Identifier ist erforderlich.", nameof(deviceIdentifier));
        }

        if (string.IsNullOrWhiteSpace(apiKeyHash))
        {
            throw new ArgumentException("Der ApiKeyHash ist erforderlich.", nameof(apiKeyHash));
        }

        Id = id;
        DeviceIdentifier = deviceIdentifier.Trim();
        ApiKeyHash = apiKeyHash.Trim();
        IstAktiv = istAktiv;
    }

    public Guid Id { get; private set; }

    public string DeviceIdentifier { get; private set; }

    public string ApiKeyHash { get; private set; }

    public bool IstAktiv { get; private set; }

    public DateTimeOffset? LetzterKontaktAm { get; private set; }

    public void SetzeKontakt(DateTimeOffset letzterKontaktAm)
    {
        LetzterKontaktAm = letzterKontaktAm;
    }

    public void SetzeAktiv(bool istAktiv)
    {
        IstAktiv = istAktiv;
    }

    public void AktualisiereApiKeyHash(string apiKeyHash)
    {
        if (string.IsNullOrWhiteSpace(apiKeyHash))
        {
            throw new ArgumentException("Der ApiKeyHash ist erforderlich.", nameof(apiKeyHash));
        }

        ApiKeyHash = apiKeyHash.Trim();
    }
}