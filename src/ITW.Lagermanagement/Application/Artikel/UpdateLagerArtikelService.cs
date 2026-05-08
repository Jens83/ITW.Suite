using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Application.Artikel;

public sealed record UpdateLagerArtikelCommand(
    Guid ArtikelId,
    string Name,
    ArtikelKategorie Kategorie,
    string BasisEinheit,
    int? PackungsGroesse,
    string? PackungsEinheit,
    decimal? VerbrauchProPatient,
    bool HatAblaufdatum,
    int Mindestbestand,
    string AktualisiertVonUserId);

public sealed class UpdateLagerArtikelService
{
    private readonly ILagerArtikelRepository _repository;

    public UpdateLagerArtikelService(ILagerArtikelRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> ExecuteAsync(
        UpdateLagerArtikelCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var artikel = await _repository.GetByIdAsync(command.ArtikelId, cancellationToken);
        if (artikel is null)
            return (false, "Artikel wurde nicht gefunden.");

        if (await _repository.ExistsByNameAsync(command.Name, command.ArtikelId, cancellationToken))
            return (false, $"Ein anderer Artikel mit dem Namen '{command.Name}' existiert bereits.");

        artikel.Aktualisiere(
            command.Name,
            command.Kategorie,
            command.BasisEinheit,
            command.PackungsGroesse,
            command.PackungsEinheit,
            command.VerbrauchProPatient,
            command.HatAblaufdatum,
            command.Mindestbestand,
            command.AktualisiertVonUserId,
            DateTimeOffset.UtcNow);

        await _repository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
