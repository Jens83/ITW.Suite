using ITW.Application.Aktivitaet;
using ITW.Application.Aufgaben;
using ITW.Application.Organisation.Contracts;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Domain.Personnel.Enums;

namespace ITW.Application.Personnel.Urlaub;

public sealed record EntscheidenUrlaubsAntragCommand(
    Guid ZeitraumId,
    bool Genehmigt,
    string? Begruendung,
    string? Loesung,
    string WachleiterUserId,
    string WachleiterAnzeigeName,
    string MitarbeiterAnzeigeName);

// Enthält den Zeitraum des entschiedenen Antrags (für Dienstwunsch-Bereinigung im Web-Layer)
public sealed record EntscheidenUrlaubsAntragResult
{
    private EntscheidenUrlaubsAntragResult(
        bool isSuccess,
        string? errorMessage,
        string? mitarbeiterUserId = null,
        DateOnly von = default,
        DateOnly bis = default)
    {
        IsSuccess        = isSuccess;
        ErrorMessage     = errorMessage;
        MitarbeiterUserId = mitarbeiterUserId;
        GenehmigterVon   = von;
        GenehmigterBis   = bis;
    }

    public bool    IsSuccess         { get; }
    public string? ErrorMessage      { get; }
    public string? MitarbeiterUserId  { get; }
    public DateOnly GenehmigterVon   { get; }
    public DateOnly GenehmigterBis   { get; }

    public static EntscheidenUrlaubsAntragResult Erfolg(string mitarbeiterUserId, DateOnly von, DateOnly bis)
        => new(true, null, mitarbeiterUserId, von, bis);

    public static EntscheidenUrlaubsAntragResult Fehler(string errorMessage)
        => new(false, errorMessage);
}

public sealed class EntscheidenUrlaubsAntragService
{
    private readonly IMitarbeiterUrlaubszeitraumRepository _urlaubszeitraumRepository;
    private readonly IAufgabeRepository _aufgabenRepository;
    private readonly IAktivitaetsLogRepository _aktivitaetsLog;

    public EntscheidenUrlaubsAntragService(
        IMitarbeiterUrlaubszeitraumRepository urlaubszeitraumRepository,
        IAufgabeRepository aufgabenRepository,
        IAktivitaetsLogRepository aktivitaetsLog)
    {
        ArgumentNullException.ThrowIfNull(urlaubszeitraumRepository);
        _urlaubszeitraumRepository = urlaubszeitraumRepository;

        ArgumentNullException.ThrowIfNull(aufgabenRepository);
        _aufgabenRepository = aufgabenRepository;

        ArgumentNullException.ThrowIfNull(aktivitaetsLog);
        _aktivitaetsLog = aktivitaetsLog;
    }

    public async Task<EntscheidenUrlaubsAntragResult> ExecuteAsync(
        EntscheidenUrlaubsAntragCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ZeitraumId == Guid.Empty)
            return EntscheidenUrlaubsAntragResult.Fehler("Der Urlaubsantrag konnte nicht ermittelt werden.");

        if (string.IsNullOrWhiteSpace(command.WachleiterUserId))
            return EntscheidenUrlaubsAntragResult.Fehler("Der Bearbeiter konnte nicht ermittelt werden.");

        var zeitraum = await _urlaubszeitraumRepository.GetByIdAsync(command.ZeitraumId, cancellationToken);

        if (zeitraum is null)
            return EntscheidenUrlaubsAntragResult.Fehler("Der Urlaubsantrag wurde nicht gefunden.");

        if (zeitraum.Status != UrlaubszeitraumStatus.Ausstehend)
            return EntscheidenUrlaubsAntragResult.Fehler("Dieser Urlaubsantrag wurde bereits entschieden.");

        var jetzt = DateTimeOffset.UtcNow;

        zeitraum.EntschiedenAm        = jetzt;
        zeitraum.EntschiedenVonUserId = command.WachleiterUserId.Trim();
        zeitraum.AktualisiertAmUtc    = jetzt;

        if (command.Genehmigt)
        {
            zeitraum.Status = UrlaubszeitraumStatus.Genehmigt;
            // Dienstwunsch-Bereinigung erfolgt im aufrufenden Web-Controller (hat Zugriff auf IDienstwunschRepository)
        }
        else
        {
            zeitraum.Status     = UrlaubszeitraumStatus.Abgelehnt;
            zeitraum.Begruendung = string.IsNullOrWhiteSpace(command.Begruendung)
                ? null
                : command.Begruendung.Trim();
            zeitraum.Loesung = string.IsNullOrWhiteSpace(command.Loesung)
                ? null
                : command.Loesung.Trim();
        }

        await _urlaubszeitraumRepository.AddOrUpdateAsync(zeitraum, cancellationToken);

        // Aufgabe als erledigt markieren
        var aufgabeSchluessel = $"itw:urlaub-genehmigen:{command.ZeitraumId}";
        var aufgabe = await _aufgabenRepository.GetBySystemSchluesselAsync(aufgabeSchluessel, cancellationToken);
        if (aufgabe is not null)
        {
            aufgabe.AlsErledigtMarkieren(jetzt);
            await _aufgabenRepository.SaveChangesAsync(cancellationToken);
        }

        // Aktivitätslog
        var entscheidungText = command.Genehmigt ? "genehmigt" : "abgelehnt";
        var logEintrag = new AktivitaetsEintrag(
            Guid.NewGuid(),
            OrganisationsbereichCode.Intensivtransport,
            $"Urlaub von {command.MitarbeiterAnzeigeName} {entscheidungText}",
            command.Genehmigt ? AktivitaetsKategorie.Erfolg : AktivitaetsKategorie.Warnung,
            command.Genehmigt ? "bi bi-calendar-check" : "bi bi-calendar-x",
            jetzt);

        await _aktivitaetsLog.AddAsync(logEintrag, cancellationToken);
        await _aktivitaetsLog.SaveChangesAsync(cancellationToken);

        return EntscheidenUrlaubsAntragResult.Erfolg(zeitraum.UserId, zeitraum.Von, zeitraum.Bis);
    }
}
