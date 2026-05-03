// Datei: src/ITW.Application/Users/CountOffenePasswortResetAnfragen/CountOffenePasswortResetAnfragenService.cs
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.CountOffenePasswortResetAnfragen;

public sealed class CountOffenePasswortResetAnfragenService
{
    private readonly IPasswortResetAnfrageRepository _passwortResetAnfrageRepository;

    public CountOffenePasswortResetAnfragenService(
        IPasswortResetAnfrageRepository passwortResetAnfrageRepository)
    {
        _passwortResetAnfrageRepository = passwortResetAnfrageRepository
            ?? throw new ArgumentNullException(nameof(passwortResetAnfrageRepository));
    }

    public async Task<CountOffenePasswortResetAnfragenResult> ExecuteAsync(
        CountOffenePasswortResetAnfragenQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.Bereich == OrganisationsbereichCode.Unbekannt)
        {
            return CountOffenePasswortResetAnfragenResult.Fehler(
                "Der Bereich für die Passwort-Reset-Zählung ist ungültig.");
        }

        var anzahl = await _passwortResetAnfrageRepository.CountOffeneAnfragenByBereichAsync(
            query.Bereich.ToDomain(),
            cancellationToken);

        return CountOffenePasswortResetAnfragenResult.Erfolg(anzahl);
    }
}