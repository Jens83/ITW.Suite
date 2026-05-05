using ITW.Fahrzeugmanagement.Application.Contracts;

namespace ITW.Fahrzeugmanagement.Application.Tracking;

public sealed record CompleteTrackingGeraetSetupCommand(
    string Einrichtungscode);

public sealed class CompleteTrackingGeraetSetupResult
{
    private CompleteTrackingGeraetSetupResult(
        bool isSuccess,
        string? errorMessage,
        string? deviceIdentifier,
        string? apiKey)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        DeviceIdentifier = deviceIdentifier;
        ApiKey = apiKey;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public string? DeviceIdentifier { get; }

    public string? ApiKey { get; }

    public static CompleteTrackingGeraetSetupResult Erfolg(
        string deviceIdentifier,
        string apiKey)
        => new(true, null, deviceIdentifier, apiKey);

    public static CompleteTrackingGeraetSetupResult Fehler(string errorMessage)
        => new(false, errorMessage, null, null);
}

public sealed class CompleteTrackingGeraetSetupService
{
    private readonly IFahrzeugTrackingRepository _repository;
    private readonly RegisterTrackingGeraetService _registerTrackingGeraetService;

    public CompleteTrackingGeraetSetupService(
        IFahrzeugTrackingRepository repository,
        RegisterTrackingGeraetService registerTrackingGeraetService)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
        ArgumentNullException.ThrowIfNull(registerTrackingGeraetService);
        _registerTrackingGeraetService = registerTrackingGeraetService;
    }

    public async Task<CompleteTrackingGeraetSetupResult> ExecuteAsync(
        CompleteTrackingGeraetSetupCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var normalisierterCode = CreateTrackingGeraetSetupCodeService
            .NormalisiereEinrichtungscode(command.Einrichtungscode);

        if (normalisierterCode.Length != 6)
        {
            return CompleteTrackingGeraetSetupResult.Fehler(
                "Der Einrichtungscode muss 6 Ziffern enthalten.");
        }

        var codeHash = CreateTrackingGeraetSetupCodeService
            .BerechneCodeHash(normalisierterCode);

        var jetztUtc = DateTimeOffset.UtcNow;

        var einrichtungscode = await _repository.GetAktivenEinrichtungscodeByCodeHashAsync(
            codeHash,
            jetztUtc,
            cancellationToken);

        if (einrichtungscode is null)
        {
            return CompleteTrackingGeraetSetupResult.Fehler(
                "Der Einrichtungscode ist ungültig, bereits verwendet oder abgelaufen.");
        }

        var registrierung = await _registerTrackingGeraetService.ExecuteAsync(
            new RegisterTrackingGeraetCommand(einrichtungscode.DeviceIdentifier),
            cancellationToken);

        if (!registrierung.IsSuccess ||
            string.IsNullOrWhiteSpace(registrierung.DeviceIdentifier) ||
            string.IsNullOrWhiteSpace(registrierung.ApiKey))
        {
            return CompleteTrackingGeraetSetupResult.Fehler(
                registrierung.ErrorMessage ?? "Das Tablet konnte nicht eingerichtet werden.");
        }

        einrichtungscode.MarkiereAlsEingeloest(jetztUtc);

        await _repository.SaveChangesAsync(cancellationToken);

        return CompleteTrackingGeraetSetupResult.Erfolg(
            registrierung.DeviceIdentifier,
            registrierung.ApiKey);
    }
}