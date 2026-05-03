using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.ReadUserOrganisationskontext;

public sealed class ReadUserOrganisationskontextService
{
    private readonly IBenutzerBereichszuordnungRepository _repository;

    public ReadUserOrganisationskontextService(
        IBenutzerBereichszuordnungRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ReadUserOrganisationskontextResult> ExecuteAsync(
        ReadUserOrganisationskontextQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (string.IsNullOrWhiteSpace(query.UserId))
        {
            return ReadUserOrganisationskontextResult.Fehler(
                "Die UserId des Mitarbeiters ist leer.");
        }

        var zuordnung = await _repository.GetAktivePrimaereZuordnungAsync(
            query.UserId,
            cancellationToken);

        if (zuordnung is null)
        {
            return ReadUserOrganisationskontextResult.Fehler(
                "Für den Mitarbeiter wurde keine aktive primäre Bereichszuordnung gefunden.");
        }

        var benutzer = new BenutzerOrganisationskontextDto(
            zuordnung.UserId,
            zuordnung.Bereich.ToApplication(),
            zuordnung.Rolle.ToApplication(),
            zuordnung.Fuehrungsverantwortung.ToApplication());

        return ReadUserOrganisationskontextResult.Erfolg(benutzer);
    }
}