using System.Security.Cryptography;
using System.Text;
using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;

namespace ITW.Fahrzeugmanagement.Application.Tracking;

public sealed record CreateTrackingGeraetSetupCodeCommand(
    string TabletName,
    string? ErstelltVonUserId);

public sealed class CreateTrackingGeraetSetupCodeResult
{
    private CreateTrackingGeraetSetupCodeResult(
        bool isSuccess,
        string? errorMessage,
        string? tabletName,
        string? deviceIdentifier,
        string? einrichtungscode,
        string? einrichtungscodeAnzeige,
        DateTimeOffset? gueltigBisUtc)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        TabletName = tabletName;
        DeviceIdentifier = deviceIdentifier;
        Einrichtungscode = einrichtungscode;
        EinrichtungscodeAnzeige = einrichtungscodeAnzeige;
        GueltigBisUtc = gueltigBisUtc;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public string? TabletName { get; }

    public string? DeviceIdentifier { get; }

    public string? Einrichtungscode { get; }

    public string? EinrichtungscodeAnzeige { get; }

    public DateTimeOffset? GueltigBisUtc { get; }

    public static CreateTrackingGeraetSetupCodeResult Erfolg(
        string tabletName,
        string deviceIdentifier,
        string einrichtungscode,
        string einrichtungscodeAnzeige,
        DateTimeOffset gueltigBisUtc)
        => new(
            true,
            null,
            tabletName,
            deviceIdentifier,
            einrichtungscode,
            einrichtungscodeAnzeige,
            gueltigBisUtc);

    public static CreateTrackingGeraetSetupCodeResult Fehler(string errorMessage)
        => new(false, errorMessage, null, null, null, null, null);
}

public sealed class CreateTrackingGeraetSetupCodeService
{
    private static readonly TimeSpan Gueltigkeitsdauer = TimeSpan.FromMinutes(20);

    private readonly IFahrzeugTrackingRepository _repository;

    public CreateTrackingGeraetSetupCodeService(IFahrzeugTrackingRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<CreateTrackingGeraetSetupCodeResult> ExecuteAsync(
        CreateTrackingGeraetSetupCodeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tabletName = command.TabletName?.Trim();

        if (string.IsNullOrWhiteSpace(tabletName))
        {
            return CreateTrackingGeraetSetupCodeResult.Fehler(
                "Bitte einen Namen für das Tablet eingeben.");
        }

        var jetztUtc = DateTimeOffset.UtcNow;
        var gueltigBisUtc = jetztUtc.Add(Gueltigkeitsdauer);

        var deviceIdentifier = ErzeugeDeviceIdentifier(tabletName);
        var einrichtungscode = ErzeugeEinrichtungscode();
        var codeHash = BerechneCodeHash(einrichtungscode);

        var entity = new TrackingGeraetEinrichtungscode(
            Guid.NewGuid(),
            tabletName,
            deviceIdentifier,
            codeHash,
            gueltigBisUtc,
            jetztUtc,
            command.ErstelltVonUserId);

        await _repository.AddEinrichtungscodeAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return CreateTrackingGeraetSetupCodeResult.Erfolg(
            tabletName,
            deviceIdentifier,
            einrichtungscode,
            FormatiereEinrichtungscode(einrichtungscode),
            gueltigBisUtc);
    }

    internal static string BerechneCodeHash(string einrichtungscode)
    {
        var normalisiert = NormalisiereEinrichtungscode(einrichtungscode);

        var bytes = Encoding.UTF8.GetBytes(normalisiert);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }

    internal static string NormalisiereEinrichtungscode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value.Where(char.IsDigit).ToArray());
    }

    private static string ErzeugeEinrichtungscode()
    {
        var zahl = RandomNumberGenerator.GetInt32(0, 1_000_000);

        return zahl.ToString("D6");
    }

    private static string FormatiereEinrichtungscode(string code)
    {
        var normalisiert = NormalisiereEinrichtungscode(code);

        return normalisiert.Length == 6
            ? $"{normalisiert[..3]}-{normalisiert[3..]}"
            : normalisiert;
    }

    private static string ErzeugeDeviceIdentifier(string tabletName)
    {
        var zufallsTeil = Convert.ToHexString(RandomNumberGenerator.GetBytes(3));

        return $"ITW-TAB-{zufallsTeil}";
    }

   
}