using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;
using ITW.Application.Organisation.VisibilityScopes;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Users.ReadUsersByScope;

public sealed class ReadUsersByScopeService
{
    private readonly IBenutzerBereichszuordnungRepository _repository;
    private readonly IBenutzerkontoRepository _benutzerkontoRepository;
    private readonly BenutzerSichtbarkeitsScopeErmittler _scopeErmittler;
    private readonly ILogger<ReadUsersByScopeService> _logger;

    public ReadUsersByScopeService(
        IBenutzerBereichszuordnungRepository repository,
        IBenutzerkontoRepository benutzerkontoRepository,
        BenutzerSichtbarkeitsScopeErmittler scopeErmittler,
        ILogger<ReadUsersByScopeService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
        ArgumentNullException.ThrowIfNull(benutzerkontoRepository);
        _benutzerkontoRepository = benutzerkontoRepository;
        ArgumentNullException.ThrowIfNull(scopeErmittler);
        _scopeErmittler = scopeErmittler;
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<ReadUsersByScopeResult> ExecuteAsync(
        ReadUsersByScopeQuery query,
        CancellationToken cancellationToken = default)
    {
        var scope = _scopeErmittler.ErmittleFuerBenutzerlisten(
            query.AufrufenderBereich,
            query.AufrufendeRolle);

        if (!scope.DarfBenutzerlistenLesen || scope.Zielbereich is null)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(ReadUsersByScopeService), scope.ErrorMessage);
            return ReadUsersByScopeResult.Fehler(
                scope.ErrorMessage ?? "Der Sichtbarkeits-Scope konnte nicht ermittelt werden.");
        }

        var zuordnungen = await _repository.GetAktivePrimaereZuordnungenByBereichAsync(
            scope.Zielbereich.Value.ToDomain(),
            cancellationToken);

        var userIds = zuordnungen
            .Select(x => x.UserId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var benutzerkonten = await _benutzerkontoRepository.GetByIdsAsync(
            userIds,
            cancellationToken);

        var benutzerkontenById = benutzerkonten.ToDictionary(
            x => x.UserId,
            StringComparer.OrdinalIgnoreCase);

        var benutzer = zuordnungen
            .Select(x =>
            {
                benutzerkontenById.TryGetValue(x.UserId, out var konto);

                return new BenutzerBereichsuebersichtDto(
                    x.Id,
                    x.UserId,
                    konto?.Benutzername ?? x.UserId,
                    konto?.Email ?? string.Empty,
                    x.Bereich.ToApplication(),
                    x.Rolle.ToApplication(),
                    x.Fuehrungsverantwortung.ToApplication(),
                    x.IsPrimary,
                    x.IsActive,
                    konto?.IstGesperrt ?? false,
                    x.ZugewiesenAm);
            })
            .OrderBy(x => x.Benutzername, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Email, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return ReadUsersByScopeResult.Erfolg(scope.Zielbereich.Value, benutzer);
    }
}