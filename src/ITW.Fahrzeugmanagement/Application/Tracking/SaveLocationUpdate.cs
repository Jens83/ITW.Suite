using System.Security.Cryptography;
using System.Text;
using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;

namespace ITW.Fahrzeugmanagement.Application.Tracking;

public sealed record SaveLocationUpdateCommand(
    string DeviceIdentifier,
    string ApiKey,
    decimal Latitude,
    decimal Longitude,
    decimal SpeedKmh,
    DateTimeOffset ErfasstAmUtc);

public sealed class SaveLocationUpdateResult
{
    private SaveLocationUpdateResult(
        bool isSuccess,
        bool isUnauthorized,
        string? errorMessage,
        Guid? trackingGeraetId,
        string? deviceIdentifier,
        Guid? routeSessionId,
        bool historieGeschrieben)
    {
        IsSuccess = isSuccess;
        IsUnauthorized = isUnauthorized;
        ErrorMessage = errorMessage;
        TrackingGeraetId = trackingGeraetId;
        DeviceIdentifier = deviceIdentifier;
        RouteSessionId = routeSessionId;
        HistorieGeschrieben = historieGeschrieben;
    }

    public bool IsSuccess { get; }

    public bool IsUnauthorized { get; }

    public string? ErrorMessage { get; }

    public Guid? TrackingGeraetId { get; }

    public string? DeviceIdentifier { get; }

    public Guid? RouteSessionId { get; }

    public bool HistorieGeschrieben { get; }

    public static SaveLocationUpdateResult Erfolg(
        Guid trackingGeraetId,
        string deviceIdentifier,
        Guid routeSessionId,
        bool historieGeschrieben)
        => new(
            isSuccess: true,
            isUnauthorized: false,
            errorMessage: null,
            trackingGeraetId: trackingGeraetId,
            deviceIdentifier: deviceIdentifier,
            routeSessionId: routeSessionId,
            historieGeschrieben: historieGeschrieben);

    public static SaveLocationUpdateResult Fehler(string message)
        => new(
            isSuccess: false,
            isUnauthorized: false,
            errorMessage: message,
            trackingGeraetId: null,
            deviceIdentifier: null,
            routeSessionId: null,
            historieGeschrieben: false);

    public static SaveLocationUpdateResult Unauthorized(string message)
        => new(
            isSuccess: false,
            isUnauthorized: true,
            errorMessage: message,
            trackingGeraetId: null,
            deviceIdentifier: null,
            routeSessionId: null,
            historieGeschrieben: false);
}

public sealed class SaveLocationUpdateService
{
    private static readonly TimeSpan NeueRouteSessionNach = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan HistorieZeitabstand = TimeSpan.FromSeconds(60);
    private const double HistorieMindestDistanzMeter = 50d;
    private const decimal HistorieGeschwindigkeitsAenderungKmh = 10m;

    private readonly IFahrzeugTrackingRepository _fahrzeugTrackingRepository;

    public SaveLocationUpdateService(IFahrzeugTrackingRepository fahrzeugTrackingRepository)
    {
        _fahrzeugTrackingRepository = fahrzeugTrackingRepository
            ?? throw new ArgumentNullException(nameof(fahrzeugTrackingRepository));
    }

    public async Task<SaveLocationUpdateResult> ExecuteAsync(
        SaveLocationUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.DeviceIdentifier))
        {
            return SaveLocationUpdateResult.Fehler("Der Device-Identifier ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.ApiKey))
        {
            return SaveLocationUpdateResult.Fehler("Der API-Key ist erforderlich.");
        }

        if (command.Latitude < -90m || command.Latitude > 90m)
        {
            return SaveLocationUpdateResult.Fehler("Die Latitude liegt außerhalb des gültigen Bereichs.");
        }

        if (command.Longitude < -180m || command.Longitude > 180m)
        {
            return SaveLocationUpdateResult.Fehler("Die Longitude liegt außerhalb des gültigen Bereichs.");
        }

        if (command.SpeedKmh < 0m)
        {
            return SaveLocationUpdateResult.Fehler("Die Geschwindigkeit darf nicht negativ sein.");
        }

        if (command.ErfasstAmUtc == default)
        {
            return SaveLocationUpdateResult.Fehler("Der Erfassungszeitpunkt ist erforderlich.");
        }

        var deviceIdentifier = command.DeviceIdentifier.Trim();
        var apiKey = command.ApiKey.Trim();
        var erfasstAmUtc = command.ErfasstAmUtc.ToUniversalTime();
        var aktualisiertAmUtc = DateTimeOffset.UtcNow;

        var trackingGeraet = await _fahrzeugTrackingRepository.GetTrackingGeraetByDeviceIdentifierAsync(
            deviceIdentifier,
            cancellationToken);

        if (trackingGeraet is null)
        {
            return SaveLocationUpdateResult.Unauthorized("Das Tracking-Gerät ist nicht registriert.");
        }

        if (!trackingGeraet.IstAktiv)
        {
            return SaveLocationUpdateResult.Unauthorized("Das Tracking-Gerät ist nicht aktiv.");
        }

        if (!IstApiKeyGueltig(trackingGeraet.ApiKeyHash, apiKey))
        {
            return SaveLocationUpdateResult.Unauthorized("Der API-Key ist ungültig.");
        }

        var aktuellerStandort = await _fahrzeugTrackingRepository.GetAktuellenTrackingGeraetStandortAsync(
            trackingGeraet.Id,
            cancellationToken);

        var letzteHistorie = await _fahrzeugTrackingRepository.GetLetztenTrackingGeraetHistorienpunktAsync(
            trackingGeraet.Id,
            cancellationToken);

        var routeSessionId = ErmittleRouteSessionId(
            aktuellerStandort,
            erfasstAmUtc);

        if (aktuellerStandort is null)
        {
            aktuellerStandort = new TrackingGeraetStandortAktuell(
                trackingGeraet.Id,
                routeSessionId,
                command.Latitude,
                command.Longitude,
                command.SpeedKmh,
                erfasstAmUtc,
                deviceIdentifier,
                aktualisiertAmUtc);

            await _fahrzeugTrackingRepository.AddAktuellenTrackingGeraetStandortAsync(
                aktuellerStandort,
                cancellationToken);
        }
        else
        {
            aktuellerStandort.Aktualisiere(
                routeSessionId,
                command.Latitude,
                command.Longitude,
                command.SpeedKmh,
                erfasstAmUtc,
                deviceIdentifier,
                aktualisiertAmUtc);
        }

        var historieSchreiben = SollHistorieGeschriebenWerden(
            letzteHistorie,
            routeSessionId,
            command.Latitude,
            command.Longitude,
            command.SpeedKmh,
            erfasstAmUtc);

        if (historieSchreiben)
        {
            var historienpunkt = new TrackingGeraetStandortHistorienpunkt(
                trackingGeraet.Id,
                routeSessionId,
                command.Latitude,
                command.Longitude,
                command.SpeedKmh,
                erfasstAmUtc,
                deviceIdentifier);

            await _fahrzeugTrackingRepository.AddTrackingGeraetHistorienpunktAsync(
                historienpunkt,
                cancellationToken);
        }

        trackingGeraet.SetzeKontakt(aktualisiertAmUtc);

        await _fahrzeugTrackingRepository.SaveChangesAsync(cancellationToken);

        return SaveLocationUpdateResult.Erfolg(
            trackingGeraet.Id,
            trackingGeraet.DeviceIdentifier,
            routeSessionId,
            historieSchreiben);
    }

    private static Guid ErmittleRouteSessionId(
        TrackingGeraetStandortAktuell? aktuellerStandort,
        DateTimeOffset erfasstAmUtc)
    {
        if (aktuellerStandort is null)
        {
            return Guid.NewGuid();
        }

        var differenz = erfasstAmUtc - aktuellerStandort.ErfasstAmUtc;

        if (differenz >= NeueRouteSessionNach)
        {
            return Guid.NewGuid();
        }

        return aktuellerStandort.RouteSessionId;
    }

    private static bool SollHistorieGeschriebenWerden(
        TrackingGeraetStandortHistorienpunkt? letzteHistorie,
        Guid routeSessionId,
        decimal latitude,
        decimal longitude,
        decimal speedKmh,
        DateTimeOffset erfasstAmUtc)
    {
        if (letzteHistorie is null)
        {
            return true;
        }

        if (letzteHistorie.RouteSessionId != routeSessionId)
        {
            return true;
        }

        if (erfasstAmUtc - letzteHistorie.ErfasstAmUtc >= HistorieZeitabstand)
        {
            return true;
        }

        var distanzMeter = BerechneDistanzInMetern(
            letzteHistorie.Latitude,
            letzteHistorie.Longitude,
            latitude,
            longitude);

        if (distanzMeter >= HistorieMindestDistanzMeter)
        {
            return true;
        }

        if (decimal.Abs(letzteHistorie.SpeedKmh - speedKmh) >= HistorieGeschwindigkeitsAenderungKmh)
        {
            return true;
        }

        return false;
    }

    private static bool IstApiKeyGueltig(string gespeicherterHash, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(gespeicherterHash) || string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        var berechneterHash = BerechneSha256Hex(apiKey);

        return string.Equals(
            gespeicherterHash.Trim(),
            berechneterHash,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string BerechneSha256Hex(string wert)
    {
        var bytes = Encoding.UTF8.GetBytes(wert);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private static double BerechneDistanzInMetern(
        decimal latitude1,
        decimal longitude1,
        decimal latitude2,
        decimal longitude2)
    {
        const double erdradiusMeter = 6371000d;

        var lat1 = GradZuRadiant((double)latitude1);
        var lon1 = GradZuRadiant((double)longitude1);
        var lat2 = GradZuRadiant((double)latitude2);
        var lon2 = GradZuRadiant((double)longitude2);

        var deltaLat = lat2 - lat1;
        var deltaLon = lon2 - lon1;

        var a =
            Math.Pow(Math.Sin(deltaLat / 2d), 2d) +
            Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLon / 2d), 2d);

        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));

        return erdradiusMeter * c;
    }

    private static double GradZuRadiant(double grad)
        => grad * (Math.PI / 180d);
}