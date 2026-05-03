namespace ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;

public sealed class LocationUpdateRequest
{
    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public decimal SpeedKmh { get; set; }

    public DateTimeOffset ErfasstAmUtc { get; set; }
}