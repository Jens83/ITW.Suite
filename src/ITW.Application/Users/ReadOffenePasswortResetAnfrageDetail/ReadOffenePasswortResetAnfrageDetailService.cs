// Datei: src/ITW.Application/Users/ReadOffenePasswortResetAnfrageDetail/ReadOffenePasswortResetAnfrageDetailService.cs
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.ReadOffenePasswortResetAnfrageDetail;

public sealed class ReadOffenePasswortResetAnfrageDetailService
{
    private readonly IPasswortResetAnfrageRepository _passwortResetAnfrageRepository;

    public ReadOffenePasswortResetAnfrageDetailService(
        IPasswortResetAnfrageRepository passwortResetAnfrageRepository)
    {
        _passwortResetAnfrageRepository = passwortResetAnfrageRepository
            ?? throw new ArgumentNullException(nameof(passwortResetAnfrageRepository));
    }

    public async Task<ReadOffenePasswortResetAnfrageDetailResult> ExecuteAsync(
        ReadOffenePasswortResetAnfrageDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.AnfrageId == Guid.Empty)
        {
            return ReadOffenePasswortResetAnfrageDetailResult.Fehler(
                "Die Passwort-Reset-Anfrage ist ungültig.");
        }

        var anfrage = await _passwortResetAnfrageRepository.GetOffeneAnfrageByIdAsync(
            query.AnfrageId,
            cancellationToken);

        if (anfrage is null)
        {
            return ReadOffenePasswortResetAnfrageDetailResult.Fehler(
                "Die offene Passwort-Reset-Anfrage wurde nicht gefunden oder wurde bereits bearbeitet.");
        }

        var dto = new OffenePasswortResetAnfrageDetailDto(
            anfrage.Id,
            anfrage.UserId,
            anfrage.Benutzername,
            anfrage.Vorname,
            anfrage.Nachname,
            anfrage.Bereich.ToApplication(),
            anfrage.AngefordertAm);

        return ReadOffenePasswortResetAnfrageDetailResult.Erfolg(dto);
    }
}