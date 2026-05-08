using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Application.Artikel;

public sealed record CreateLagerArtikelCommand(
    string Name,
    ArtikelKategorie Kategorie,
    string BasisEinheit,
    int? PackungsGroesse,
    string? PackungsEinheit,
    decimal? VerbrauchProPatient,
    bool HatAblaufdatum,
    int Mindestbestand,
    string ErstelltVonUserId);

public sealed class CreateLagerArtikelResult
{
    private CreateLagerArtikelResult(bool isSuccess, string? errorMessage, Guid artikelId = default)
    {
        IsSuccess    = isSuccess;
        ErrorMessage = errorMessage;
        ArtikelId    = artikelId;
    }

    public bool    IsSuccess    { get; }
    public string? ErrorMessage { get; }
    public Guid    ArtikelId   { get; }

    public static CreateLagerArtikelResult Erfolg(Guid artikelId) => new(true, null, artikelId);
    public static CreateLagerArtikelResult Fehler(string message) => new(false, message);
}

public sealed class CreateLagerArtikelService
{
    private readonly ILagerArtikelRepository _repository;

    public CreateLagerArtikelService(ILagerArtikelRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<CreateLagerArtikelResult> ExecuteAsync(
        CreateLagerArtikelCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
            return CreateLagerArtikelResult.Fehler("Name ist erforderlich.");

        if (string.IsNullOrWhiteSpace(command.BasisEinheit))
            return CreateLagerArtikelResult.Fehler("Basiseinheit ist erforderlich.");

        if (await _repository.ExistsByNameAsync(command.Name, cancellationToken))
            return CreateLagerArtikelResult.Fehler($"Ein Artikel mit dem Namen '{command.Name}' existiert bereits.");

        var artikel = new LagerArtikel(
            Guid.NewGuid(),
            command.Name,
            command.Kategorie,
            command.BasisEinheit,
            command.PackungsGroesse,
            command.PackungsEinheit,
            command.VerbrauchProPatient,
            command.HatAblaufdatum,
            command.Mindestbestand,
            command.ErstelltVonUserId,
            DateTimeOffset.UtcNow);

        await _repository.AddAsync(artikel, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return CreateLagerArtikelResult.Erfolg(artikel.Id);
    }
}
