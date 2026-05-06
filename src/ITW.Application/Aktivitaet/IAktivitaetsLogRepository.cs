using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Aktivitaet;

public interface IAktivitaetsLogRepository
{
    Task<IReadOnlyList<AktivitaetsEintrag>> GetByBereichAsync(
        OrganisationsbereichCode bereich,
        int maxAnzahl = 10,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        AktivitaetsEintrag eintrag,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
