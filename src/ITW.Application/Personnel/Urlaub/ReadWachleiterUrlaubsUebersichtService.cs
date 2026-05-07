using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Domain.Personnel.Enums;

namespace ITW.Application.Personnel.Urlaub;

public sealed class WachleiterUrlaubsAntragDto
{
    public Guid    ZeitraumId          { get; init; }
    public string  UserId              { get; init; } = string.Empty;
    public string  MitarbeiterName     { get; init; } = string.Empty;
    public DateOnly Von                { get; init; }
    public DateOnly Bis                { get; init; }
    public string? Notiz               { get; init; }
    public bool    HatUeberschneidung  { get; init; }
}

public sealed class ReadWachleiterUrlaubsUebersichtResult
{
    public bool                                IsSuccess    { get; init; } = true;
    public string?                             ErrorMessage { get; init; }
    public IReadOnlyList<WachleiterUrlaubsAntragDto> Antraege { get; init; } = [];
}

public sealed class ReadWachleiterUrlaubsUebersichtService
{
    private readonly IMitarbeiterUrlaubszeitraumRepository _urlaubszeitraumRepository;
    private readonly ReadItwMitarbeiterprofileService _profileService;

    public ReadWachleiterUrlaubsUebersichtService(
        IMitarbeiterUrlaubszeitraumRepository urlaubszeitraumRepository,
        ReadItwMitarbeiterprofileService profileService)
    {
        ArgumentNullException.ThrowIfNull(urlaubszeitraumRepository);
        _urlaubszeitraumRepository = urlaubszeitraumRepository;

        ArgumentNullException.ThrowIfNull(profileService);
        _profileService = profileService;
    }

    public async Task<ReadWachleiterUrlaubsUebersichtResult> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var ausstehende = await _urlaubszeitraumRepository.GetAllAusstehendAsync(cancellationToken);

        if (ausstehende.Count == 0)
        {
            return new ReadWachleiterUrlaubsUebersichtResult { Antraege = [] };
        }

        var profileResult = await _profileService.ExecuteAsync(cancellationToken);

        var namenByUserId = profileResult.IsSuccess
            ? profileResult.Profile.ToDictionary(
                p => p.UserId,
                p => p.AnzeigeName,
                StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Überschneidungen erkennen: zwei Anträge überschneiden sich wenn ihre Zeiträume sich berühren
        var antraegeMitFlag = ausstehende
            .OrderBy(a => a.Von)
            .Select(antrag =>
            {
                var hatUeberschneidung = ausstehende.Any(anderer =>
                    anderer.Id != antrag.Id &&
                    anderer.Von <= antrag.Bis &&
                    anderer.Bis >= antrag.Von);

                return new WachleiterUrlaubsAntragDto
                {
                    ZeitraumId         = antrag.Id,
                    UserId             = antrag.UserId,
                    MitarbeiterName    = namenByUserId.TryGetValue(antrag.UserId, out var name) ? name : antrag.UserId,
                    Von                = antrag.Von,
                    Bis                = antrag.Bis,
                    Notiz              = antrag.Notiz,
                    HatUeberschneidung = hatUeberschneidung
                };
            })
            .ToList();

        return new ReadWachleiterUrlaubsUebersichtResult { Antraege = antraegeMitFlag };
    }
}
