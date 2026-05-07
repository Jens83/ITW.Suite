using ITW.Application.Aufgaben;
using ITW.Application.Organisation.Contracts;
using ITW.Dienstplan.Application.Contracts;
using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Web.Areas.Intensivtransport.Services.Dashboard;

public sealed class SystemAufgabeGeneratorService
{
    private readonly IDienstplanPeriodeRepository _perioden;
    private readonly IAufgabeRepository _aufgaben;
    private readonly IFahrzeugRepository _fahrzeuge;
    private readonly IFahrzeugPruefungRepository _pruefungen;

    public SystemAufgabeGeneratorService(
        IDienstplanPeriodeRepository perioden,
        IAufgabeRepository aufgaben,
        IFahrzeugRepository fahrzeuge,
        IFahrzeugPruefungRepository pruefungen)
    {
        ArgumentNullException.ThrowIfNull(perioden);
        _perioden = perioden;
        ArgumentNullException.ThrowIfNull(aufgaben);
        _aufgaben = aufgaben;
        ArgumentNullException.ThrowIfNull(fahrzeuge);
        _fahrzeuge = fahrzeuge;
        ArgumentNullException.ThrowIfNull(pruefungen);
        _pruefungen = pruefungen;
    }

    public async Task AktualisierenAsync(CancellationToken cancellationToken = default)
    {
        var jetzt = DateTimeOffset.UtcNow;
        var heute = DateOnly.FromDateTime(DateTime.Today);

        await GeneriereDisenstplanAufgabenAsync(heute, jetzt, cancellationToken);
        await GenerierefahrzeugpruefungAufgabenAsync(heute, jetzt, cancellationToken);

        await _aufgaben.SaveChangesAsync(cancellationToken);
    }

    private async Task GeneriereDisenstplanAufgabenAsync(
        DateOnly heute,
        DateTimeOffset jetzt,
        CancellationToken cancellationToken)
    {
        var allePerioden = await _perioden.GetAlleAsync(cancellationToken);

        var fensterende = heute.AddMonths(2);
        var fensterbeginn = heute.AddMonths(-2);

        var relevantePerioden = allePerioden
            .Where(p => !p.PlanIstFreigegeben)
            .Where(p =>
            {
                var periodStart = new DateOnly(p.Jahr, p.Monat, 1);
                return periodStart >= fensterbeginn && periodStart <= fensterende;
            })
            .ToList();

        foreach (var periode in relevantePerioden)
        {
            var periodStart = new DateOnly(periode.Jahr, periode.Monat, 1);

            if (periode.WunschphaseIstOffen)
            {
                // Wunschphase läuft: Aufgabe "Dienstplan fertigstellen", fällig am 20. des Vormonats
                var schluessel = $"itw:dienstplan-fertigstellen:{periode.Id}";
                if (!await _aufgaben.ExistiertOffeneSystemaufgabeAsync(schluessel, cancellationToken))
                {
                    var faelligkeit = new DateOnly(periodStart.Year, periodStart.Month, 1)
                        .AddMonths(-1)
                        .AddDays(19); // 1. des Vormonats + 19 = 20. des Vormonats

                    var prioritaet = faelligkeit < heute
                        ? AufgabePrioritaet.Dringend
                        : faelligkeit <= heute.AddDays(7)
                            ? AufgabePrioritaet.Hoch
                            : AufgabePrioritaet.Normal;

                    var aufgabe = new Aufgabe(
                        Guid.NewGuid(),
                        OrganisationsbereichCode.Intensivtransport,
                        $"Dienstplan {periode.Bezeichnung} fertigstellen",
                        prioritaet,
                        AufgabeQuelle.System,
                        faelligkeit,
                        schluessel,
                        jetzt);
                    await _aufgaben.AddAsync(aufgabe, cancellationToken);
                }
                continue;
            }

            if (periodStart <= heute.AddDays(7))
            {
                var schluessel = $"itw:plan-freigeben:{periode.Id}";
                if (!await _aufgaben.ExistiertOffeneSystemaufgabeAsync(schluessel, cancellationToken))
                {
                    var aufgabe = new Aufgabe(
                        Guid.NewGuid(),
                        OrganisationsbereichCode.Intensivtransport,
                        $"Dienstplan {periode.Bezeichnung} freigeben",
                        AufgabePrioritaet.Hoch,
                        AufgabeQuelle.System,
                        periodStart,
                        schluessel,
                        jetzt);
                    await _aufgaben.AddAsync(aufgabe, cancellationToken);
                }
            }
            else
            {
                var schluessel = $"itw:wunschphase-oeffnen:{periode.Id}";
                if (!await _aufgaben.ExistiertOffeneSystemaufgabeAsync(schluessel, cancellationToken))
                {
                    var faelligkeit = periodStart.AddDays(-14);
                    var aufgabe = new Aufgabe(
                        Guid.NewGuid(),
                        OrganisationsbereichCode.Intensivtransport,
                        $"Wunschphase für {periode.Bezeichnung} öffnen",
                        AufgabePrioritaet.Normal,
                        AufgabeQuelle.System,
                        faelligkeit > heute ? faelligkeit : null,
                        schluessel,
                        jetzt);
                    await _aufgaben.AddAsync(aufgabe, cancellationToken);
                }
            }
        }
    }

    private async Task GenerierefahrzeugpruefungAufgabenAsync(
        DateOnly heute,
        DateTimeOffset jetzt,
        CancellationToken cancellationToken)
    {
        var alleFahrzeuge = await _fahrzeuge.GetFahrzeugeAsync(cancellationToken);

        foreach (var fahrzeug in alleFahrzeuge)
        {
            var fahrzeugPruefungen = await _pruefungen.GetByFahrzeugIdAsync(
                fahrzeug.Id, cancellationToken);

            foreach (var pruefung in fahrzeugPruefungen)
            {
                var tageBis = pruefung.FaelligAm.DayNumber - heute.DayNumber;

                if (tageBis > 30) continue;

                var schluessel = $"itw:fahrzeug-pruefung:{fahrzeug.Id}:{pruefung.Id}";
                if (await _aufgaben.ExistiertOffeneSystemaufgabeAsync(schluessel, cancellationToken))
                    continue;

                var prioritaet = tageBis < 0
                    ? AufgabePrioritaet.Dringend
                    : tageBis < 14
                        ? AufgabePrioritaet.Hoch
                        : AufgabePrioritaet.Normal;

                var typText = pruefung.Typ switch
                {
                    FahrzeugPruefungTyp.HuAu => "HU/AU",
                    FahrzeugPruefungTyp.SicherheitspruefungElektrischeAnlage => "Sicherheitsprüfung Elektrik",
                    FahrzeugPruefungTyp.SicherheitspruefungSauerstoffanlage => "Sicherheitsprüfung Sauerstoff",
                    FahrzeugPruefungTyp.SicherheitspruefungAufbau => "Sicherheitsprüfung Aufbau",
                    FahrzeugPruefungTyp.Service => "Service",
                    _ => "Prüfung"
                };

                var aufgabe = new Aufgabe(
                    Guid.NewGuid(),
                    OrganisationsbereichCode.Intensivtransport,
                    $"{typText} fällig – {fahrzeug.Kennzeichen}",
                    prioritaet,
                    AufgabeQuelle.System,
                    pruefung.FaelligAm,
                    schluessel,
                    jetzt);

                await _aufgaben.AddAsync(aufgabe, cancellationToken);
            }
        }
    }
}
