using ITW.Application.Aufgaben;
using ITW.Application.Organisation.Contracts;
using ITW.Dienstplan.Application.Contracts;

namespace ITW.Web.Areas.Intensivtransport.Services.Dashboard;

public sealed class SystemAufgabeGeneratorService
{
    private readonly IDienstplanPeriodeRepository _perioden;
    private readonly IAufgabeRepository _aufgaben;

    public SystemAufgabeGeneratorService(
        IDienstplanPeriodeRepository perioden,
        IAufgabeRepository aufgaben)
    {
        ArgumentNullException.ThrowIfNull(perioden);
        ArgumentNullException.ThrowIfNull(aufgaben);
        _perioden = perioden;
        _aufgaben = aufgaben;
    }

    public async Task AktualisierenAsync(CancellationToken cancellationToken = default)
    {
        var allePerioden = await _perioden.GetAlleAsync(cancellationToken);
        var jetzt        = DateTimeOffset.UtcNow;
        var heute        = DateOnly.FromDateTime(DateTime.Today);

        // Only consider periods within a relevant window (past 2 months + future 2 months)
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
                // Wunschphase is open: nothing to prompt, users are entering wishes
                continue;
            }

            // Wunschphase is closed and plan not released:
            // Suggest freigeben only if the period is current or past
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
                // Future period, Wunschphase not yet opened
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

        await _aufgaben.SaveChangesAsync(cancellationToken);
    }
}
