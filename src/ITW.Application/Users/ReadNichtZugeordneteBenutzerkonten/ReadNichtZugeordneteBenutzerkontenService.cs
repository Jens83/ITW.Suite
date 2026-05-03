using ITW.Application.Abstractions.Identity;

namespace ITW.Application.Users.ReadNichtZugeordneteBenutzerkonten;

public sealed class ReadNichtZugeordneteBenutzerkontenService
{
    private readonly IBenutzerkontoRepository _repository;

    public ReadNichtZugeordneteBenutzerkontenService(IBenutzerkontoRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ReadNichtZugeordneteBenutzerkontenResult> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var benutzerkonten = await _repository.GetNichtZugeordneteBenutzerkontenAsync(cancellationToken);

        return ReadNichtZugeordneteBenutzerkontenResult.Erfolg(benutzerkonten);
    }
}