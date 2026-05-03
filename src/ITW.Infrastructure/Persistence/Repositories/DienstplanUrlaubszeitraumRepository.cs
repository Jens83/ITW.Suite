using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Dienstplan.Application.Contracts;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class DienstplanUrlaubszeitraumRepository : IDienstplanUrlaubszeitraumRepository
{
    private readonly IMitarbeiterUrlaubszeitraumRepository _mitarbeiterUrlaubszeitraumRepository;

    public DienstplanUrlaubszeitraumRepository(
        IMitarbeiterUrlaubszeitraumRepository mitarbeiterUrlaubszeitraumRepository)
    {
        _mitarbeiterUrlaubszeitraumRepository = mitarbeiterUrlaubszeitraumRepository
            ?? throw new ArgumentNullException(nameof(mitarbeiterUrlaubszeitraumRepository));
    }

    public Task<IReadOnlyList<string>> GetAktiveUserIdsFuerDatumAsync(
        DateOnly datum,
        CancellationToken cancellationToken = default)
    {
        return _mitarbeiterUrlaubszeitraumRepository.GetAktiveUserIdsFuerDatumAsync(
            datum,
            cancellationToken);
    }
}