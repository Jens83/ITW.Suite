using ITW.Application.Abstractions.Identity;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Users.ReadNichtZugeordneteBenutzerkonten;

public sealed class ReadNichtZugeordneteBenutzerkontenService
{
    private readonly IBenutzerkontoRepository _repository;
    private readonly ILogger<ReadNichtZugeordneteBenutzerkontenService> _logger;

    public ReadNichtZugeordneteBenutzerkontenService(IBenutzerkontoRepository repository, ILogger<ReadNichtZugeordneteBenutzerkontenService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReadNichtZugeordneteBenutzerkontenResult> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var benutzerkonten = await _repository.GetNichtZugeordneteBenutzerkontenAsync(cancellationToken);

        return ReadNichtZugeordneteBenutzerkontenResult.Erfolg(benutzerkonten);
    }
}