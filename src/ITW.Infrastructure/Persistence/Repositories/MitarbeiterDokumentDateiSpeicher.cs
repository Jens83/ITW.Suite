// Datei: src/ITW.Infrastructure/Persistence/Repositories/MitarbeiterDokumentDateiSpeicher.cs
using ITW.Application.Abstractions.Persistence;
using Microsoft.Extensions.Hosting;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class MitarbeiterDokumentDateiSpeicher : IMitarbeiterDokumentDateiSpeicher
{
    private readonly string _rootPfad;

    public MitarbeiterDokumentDateiSpeicher(IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        _rootPfad = Path.Combine(
            hostEnvironment.ContentRootPath,
            "App_Data",
            "Mitarbeiterdokumente");

        Directory.CreateDirectory(_rootPfad);
    }

    public async Task<string> SpeichereAsync(
        string userId,
        string dateinameOriginal,
        byte[] dateiinhalt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Die UserId ist erforderlich.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(dateinameOriginal))
        {
            throw new ArgumentException("Der Original-Dateiname ist erforderlich.", nameof(dateinameOriginal));
        }

        ArgumentNullException.ThrowIfNull(dateiinhalt);

        var dateiendung = Path.GetExtension(dateinameOriginal)?.Trim() ?? string.Empty;
        var userOrdner = Path.Combine(_rootPfad, userId.Trim());

        Directory.CreateDirectory(userOrdner);

        var gespeicherterDateiname = $"{Guid.NewGuid():N}{dateiendung.ToLowerInvariant()}";
        var relativerPfad = Path.Combine(userId.Trim(), gespeicherterDateiname).Replace("\\", "/");
        var absoluterPfad = Path.Combine(userOrdner, gespeicherterDateiname);

        await File.WriteAllBytesAsync(absoluterPfad, dateiinhalt, cancellationToken);

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