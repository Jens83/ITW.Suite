using System.Security.Cryptography;
using System.Text;
using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;

namespace ITW.Fahrzeugmanagement.Application.Tracking;

public sealed record RegisterTrackingGeraetCommand(
    string DeviceIdentifier);

public sealed class RegisterTrackingGeraetResult
{
    private RegisterTrackingGeraetResult(
        bool isSuccess,
        string? errorMessage,
        Guid? trackingGeraetId,
        string? deviceIdentifier,
        string? apiKey)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        TrackingGeraetId = trackingGeraetId;
        DeviceIdentifier = deviceIdentifier;
        ApiKey = apiKey;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public Guid? TrackingGeraetId { get; }

    public string? DeviceIdentifier { get; }

    public string? ApiKey { get; }

    public static RegisterTrackingGeraetResult Erfolg(
        Guid trackingGeraetId,
        string deviceIdentifier,
        string apiKey)
        => new(
            true,
            null,
            trackingGeraetId,
            deviceIdentifier,
            apiKey);

    public static RegisterTrackingGeraetResult Fehler(string errorMessage)
        => new(
            false,
            errorMessage,
            null,
            null,
            null);
}

public sealed class RegisterTrackingGeraetService
{
    private readonly IFahrzeugTrackingRepository _repository;

    public RegisterTrackingGeraetService(IFahrzeugTrackingRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<RegisterTrackingGeraetResult> ExecuteAsync(
        RegisterTrackingGeraetCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.DeviceIdentifier))
        {
            return RegisterTrackingGeraetResult.Fehler("Der Device-Identifier ist erforderlich.");
        }

        var deviceIdentifier = command.DeviceIdentifier.Trim();

        var vorhandenesDevice = await _repository.GetTrackingGeraetByDeviceIdentifierAsync(
            deviceIdentifier,
            cancellationToken);

        var apiKey = GenerateApiKey();
        var apiKeyHash = BerechneSha256Hex(apiKey);

        if (vorhandenesDevice is not null)
        {
            vorhandenesDevice.AktualisiereApiKeyHash(apiKeyHash);
            vorhandenesDevice.SetzeAktiv(true);

            await _repository.SaveChangesAsync(cancellationToken);

            return RegisterTrackingGeraetResult.Erfolg(
                vorhandenesDevice.Id,
                vorhandenesDevice.DeviceIdentifier,
                apiKey);
        }

        var trackingGeraet = new FahrzeugTrackingGeraet(
            Guid.NewGuid(),
            deviceIdentifier,
            apiKeyHash,
            istAktiv: true);

        await _repository.AddTrackingGeraetAsync(
            trackingGeraet,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        return RegisterTrackingGeraetResult.Erfolg(
            trackingGeraet.Id,
            trackingGeraet.DeviceIdentifier,
            apiKey);
    }

    private static string GenerateApiKey()
    {
        Span<byte> buffer = stackalloc byte[32];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToHexString(buffer);
    }

    private static string BerechneSha256Hex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}