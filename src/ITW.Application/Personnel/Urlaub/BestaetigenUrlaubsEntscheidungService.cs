using ITW.Application.Aktivitaet;
using ITW.Application.Organisation.Contracts;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Domain.Personnel.Enums;

namespace ITW.Application.Personnel.Urlaub;

public sealed record BestaetigenUrlaubsEntscheidungCommand(
    Guid ZeitraumId,
    string MitarbeiterUserId,
    string MitarbeiterAnzeigeName);

public sealed class BestaetigenUrlaubsEntscheidungService
{
    private readonly IMitarbeiterUrlaubszeitraumRepository _urlaubszeitraumRepository;
    private readonly IAktivitaetsLogRepository _aktivitaetsLog;

    public BestaetigenUrlaubsEntscheidungService(
        IMitarbeiterUrlaubszeitraumRepository urlaubszeitraumRepository,
        IAktivitaetsLogRepository aktivitaetsLog)
    {
        ArgumentNullException.ThrowIfNull(urlaubszeitraumRepository);
        _urlaubszeitraumRepository = urlaubszeitraumRepository;

        ArgumentNullException.ThrowIfNull(aktivitaetsLog);
        _aktivitaetsLog = aktivitaetsLog;
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> ExecuteAsync(
        BestaetigenUrlaubsEntscheidungCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ZeitraumId == Guid.Empty)
            return (false, "Der Urlaubsantrag konnte nicht ermittelt werden.");

        var zeitraum = await _urlaubszeitraumRepository.GetByIdAsync(command.ZeitraumId, cancellationToken);

        if (zeitraum is null)
            return (false, "Der Urlaubsantrag wurde nicht gefunden.");

        if (!string.Equals(zeitraum.UserId, command.MitarbeiterUserId, StringComparison.OrdinalIgnoreCase))
            return (false, "Kein Zugriff auf diesen Urlaubsantrag.");

        if (zeitraum.Status == UrlaubszeitraumStatus.Ausstehend)
            return (false, "Dieser Antrag wurde noch nicht entschieden.");

        if (zeitraum.MitarbeiterBestaetigtAm.HasValue)
            return (false, "Diese Entscheidung wurde bereits bestätigt.");

        var jetzt = DateTimeOffset.UtcNow;
        zeitraum.MitarbeiterBestaetigtAm = jetzt;
        zeitraum.AktualisiertAmUtc       = jetzt;

        await _urlaubszeitraumRepository.AddOrUpdateAsync(zeitraum, cancellationToken);

        var entscheidungText = zeitraum.Status == UrlaubszeitraumStatus.Genehmigt
            ? "genehmigten"
            : "abgelehnten";

        var logEintrag = new AktivitaetsEintrag(
            Guid.NewGuid(),
            OrganisationsbereichCode.Intensivtransport,
            $"{command.MitarbeiterAnzeigeName} hat {entscheidungText} Urlaub bestätigt",
            AktivitaetsKategorie.Info,
            zeitraum.Status == UrlaubszeitraumStatus.Genehmigt
                ? "bi bi-calendar-check"
                : "bi bi-calendar-x",
            jetzt);

        await _aktivitaetsLog.AddAsync(logEintrag, cancellationToken);
        await _aktivitaetsLog.SaveChangesAsync(cancellationToken);

        return (true, null);
    }
}
