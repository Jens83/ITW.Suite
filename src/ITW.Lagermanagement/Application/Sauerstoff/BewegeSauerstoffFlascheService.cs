using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Application.Sauerstoff;

public sealed class BewegeSauerstoffFlascheService
{
    private readonly ISauerstoffFlascheRepository _repository;

    public BewegeSauerstoffFlascheService(ISauerstoffFlascheRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> NachFahrzeugAsync(
        Guid flascheId,
        Guid fahrzeugId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var flasche = await _repository.GetByIdAsync(flascheId, cancellationToken);
        if (flasche is null)
            return (false, "Flasche nicht gefunden.");

        if (!flasche.IstImDepot)
            return (false, "Die Flasche befindet sich nicht im Depot.");

        if (flasche.Status != SauerstoffFlaschenStatus.Voll)
            return (false, "Nur volle Flaschen können einem Fahrzeug zugewiesen werden.");

        var jetzt = DateTimeOffset.UtcNow;
        flasche.NachFahrzeugBewegen(fahrzeugId);

        await _repository.AddBewegungAsync(
            new SauerstoffBewegung(
                Guid.NewGuid(),
                flasche.Id,
                SauerstoffBewegungsTyp.EntnahmeNachFahrzeug,
                null,
                fahrzeugId,
                jetzt,
                userId),
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> AlsLeerZurueckgebenAsync(
        Guid flascheId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var flasche = await _repository.GetByIdAsync(flascheId, cancellationToken);
        if (flasche is null)
            return (false, "Flasche nicht gefunden.");

        if (flasche.IstImDepot)
            return (false, "Die Flasche befindet sich bereits im Depot.");

        var vonFahrzeugId = flasche.FahrzeugId;
        var jetzt         = DateTimeOffset.UtcNow;

        flasche.AlsLeerZurueckgeben();

        await _repository.AddBewegungAsync(
            new SauerstoffBewegung(
                Guid.NewGuid(),
                flasche.Id,
                SauerstoffBewegungsTyp.RueckgabeAlsLeer,
                vonFahrzeugId,
                null,
                jetzt,
                userId),
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> AbgebenAnLieferantAsync(
        IReadOnlyList<Guid> flaschenIds,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (flaschenIds.Count == 0)
            return (false, "Keine Flaschen ausgewählt.");

        var jetzt = DateTimeOffset.UtcNow;

        foreach (var id in flaschenIds)
        {
            var flasche = await _repository.GetByIdAsync(id, cancellationToken);
            if (flasche is null)
                continue;

            if (!flasche.IstImDepot || flasche.Status != SauerstoffFlaschenStatus.Leer)
                continue;

            flasche.Deaktiviere();

            await _repository.AddBewegungAsync(
                new SauerstoffBewegung(
                    Guid.NewGuid(),
                    flasche.Id,
                    SauerstoffBewegungsTyp.Abgang,
                    null,
                    null,
                    jetzt,
                    userId),
                cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
