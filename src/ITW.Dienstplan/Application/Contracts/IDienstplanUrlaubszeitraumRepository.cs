namespace ITW.Dienstplan.Application.Contracts;

public interface IDienstplanUrlaubszeitraumRepository
{
    Task<IReadOnlyList<string>> GetAktiveUserIdsFuerDatumAsync(
        DateOnly datum,
        CancellationToken cancellationToken = default);
}