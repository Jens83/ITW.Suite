using ITW.Fahrzeugmanagement.Application.Contracts;
using Microsoft.Extensions.Hosting;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class FahrzeugDokumentDateiSpeicher : IFahrzeugDokumentDateiSpeicher
{
    private readonly string _rootPfad;

    public FahrzeugDokumentDateiSpeicher(IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        _rootPfad = Path.Combine(
            hostEnvironment.ContentRootPath,
            "App_Data",
            "Fahrzeugdokumente");

        Directory.CreateDirectory(_rootPfad);
    }

    public async Task<string> SpeichereAsync(
        Guid fahrzeugId,
        string dateinameOriginal,
        byte[] dateiinhalt,
        CancellationToken cancellationToken = default)
    {
        if (fahrzeugId == Guid.Empty)
        {
            throw new ArgumentException("Die Fahrzeug-ID ist erforderlich.", nameof(fahrzeugId));
        }

        if (string.IsNullOrWhiteSpace(dateinameOriginal))
        {
            throw new ArgumentException("Der Original-Dateiname ist erforderlich.", nameof(dateinameOriginal));
        }

        ArgumentNullException.ThrowIfNull(dateiinhalt);

        var dateiendung = Path.GetExtension(dateinameOriginal)?.Trim() ?? string.Empty;
        var fahrzeugOrdnerName = fahrzeugId.ToString("N");

        var fahrzeugOrdner = Path.Combine(
            _rootPfad,
            fahrzeugOrdnerName);

        Directory.CreateDirectory(fahrzeugOrdner);

        var gespeicherterDateiname = $"{Guid.NewGuid():N}{dateiendung.ToLowerInvariant()}";

        var relativerPfad = Path.Combine(
                fahrzeugOrdnerName,
                gespeicherterDateiname)
            .Replace("\\", "/");

        var absoluterPfad = Path.Combine(
            fahrzeugOrdner,
            gespeicherterDateiname);

        await File.WriteAllBytesAsync(
            absoluterPfad,
            dateiinhalt,
            cancellationToken);

        return relativerPfad;
    }

    public Task<byte[]?> LadeAsync(
        string speicherpfad,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(speicherpfad))
        {
            return Task.FromResult<byte[]?>(null);
        }

        var absoluterPfad = Path.Combine(
            _rootPfad,
            speicherpfad.Replace("/", Path.DirectorySeparatorChar.ToString()));

        if (!File.Exists(absoluterPfad))
        {
            return Task.FromResult<byte[]?>(null);
        }

        return File.ReadAllBytesAsync(absoluterPfad, cancellationToken)!;
    }

    public Task LoescheAsync(
        string speicherpfad,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(speicherpfad))
        {
            return Task.CompletedTask;
        }

        var absoluterPfad = Path.Combine(
            _rootPfad,
            speicherpfad.Replace("/", Path.DirectorySeparatorChar.ToString()));

        if (File.Exists(absoluterPfad))
        {
            File.Delete(absoluterPfad);
        }

        return Task.CompletedTask;
    }
}