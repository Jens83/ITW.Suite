using ITW.Application.Abstractions.Identity;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Users.ReadNichtZugeordneteBenutzerkonten;

public sealed class ReadNichtZugeordneteBenutzerkontenService
{
    private readonly IBenutzerkontoRepository _repository;
    private readonly ILogger<ReadNichtZugeordneteBenutzerkontenService> _logger;

    public ReadNichtZugeordneteBenutzerkontenService(IBenutzerkontoRepository repository, ILogger<ReadNichtZugeordneteBenutzerkontenService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<ReadNichtZugeordneteBenutzerkontenResult> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var benutzerkonten = await _repository.GetNichtZugeordneteBenutzerkontenAsync(cancellationToken);

        return ReadNichtZugeordneteBenutzerkontenResult.Erfolg(benutzerkonten);
    }
}