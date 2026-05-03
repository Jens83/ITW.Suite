using ITW.Domain.Personnel.Entities;

namespace ITW.Application.Personnel.Urlaub.Contracts;

public interface IMitarbeiterUrlaubsanspruchRepository
{
    Task<MitarbeiterUrlaubsanspruch?> GetAsync(
        string userId,
        int jahr,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MitarbeiterUrlaubsanspruch>> GetAlleFuerBenutzerAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        MitarbeiterUrlaubsanspruch anspruch,
        CancellationToken cancellationToken = default);
}