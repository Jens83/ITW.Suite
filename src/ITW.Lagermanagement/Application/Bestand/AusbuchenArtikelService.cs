using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Application.Bestand;

public sealed record AusbuchenArtikelCommand(
    Guid ArtikelId,
    Lagerort Lagerort,
    int MengeInBasiseinheiten,
    string AusgebuchtVonUserId,
    // Für Chargen-Artikel: optionale Chargen-Id, sonst FIFO
    Guid? ChargeId = null);

public sealed class AusbuchenArtikelService
{
    private readonly ILagerArtikelRepository  _artikelRepo;
    private readonly IArtikelBestandRepository _bestandRepo;
    private readonly IArtikelChargeRepository  _chargeRepo;

    public AusbuchenArtikelService(
        ILagerArtikelRepository artikelRepo,
        IArtikelBestandRepository bestandRepo,
        IArtikelChargeRepository chargeRepo)
    {
        ArgumentNullException.ThrowIfNull(artikelRepo);
        ArgumentNullException.ThrowIfNull(bestandRepo);
        ArgumentNullException.ThrowIfNull(chargeRepo);
        _artikelRepo = artikelRepo;
        _bestandRepo = bestandRepo;
        _chargeRepo  = chargeRepo;
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> ExecuteAsync(
        AusbuchenArtikelCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.MengeInBasiseinheiten <= 0)
            return (false, "Menge muss größer als 0 sein.");

        var artikel = await _artikelRepo.GetByIdAsync(command.ArtikelId, cancellationToken);
        if (artikel is null)
            return (false, "Artikel wurde nicht gefunden.");

        if (artikel.HatAblaufdatum)
        {
            var heute = DateOnly.FromDateTime(DateTime.Today);

            if (command.ChargeId.HasValue)
            {
                var charge = await _chargeRepo.GetByIdAsync(command.ChargeId.Value, cancellationToken);
                if (charge is null || charge.ArtikelId != command.ArtikelId)
                    return (false, "Charge wurde nicht gefunden.");
                if (charge.IstAusgebucht)
                    return (false, "Diese Charge wurde bereits vollständig ausgebucht.");
                if (charge.Menge < command.MengeInBasiseinheiten)
                    return (false, $"Nicht genügend Bestand in der Charge. Verfügbar: {charge.Menge}.");

                // Vollständige Ausbuchen wenn Restmenge = 0
                if (charge.Menge == command.MengeInBasiseinheiten)
                    charge.Ausbuchen(command.AusgebuchtVonUserId, heute);
                else
                    charge.MengeAnpassen(charge.Menge - command.MengeInBasiseinheiten);
            }
            else
            {
                // FIFO: älteste Charge zuerst
                var chargen = await _chargeRepo.GetAktiveByArtikelUndLagerortAsync(
                    command.ArtikelId, command.Lagerort, cancellationToken);

                var geordnet = chargen
                    .Where(c => !c.IstAusgebucht)
                    .OrderBy(c => c.Ablaufdatum)
                    .ThenBy(c => c.EingebuchtAm)
                    .ToList();

                var gesamtVerfuegbar = geordnet.Sum(c => c.Menge);
                if (gesamtVerfuegbar < command.MengeInBasiseinheiten)
                    return (false, $"Nicht genügend Bestand. Verfügbar: {gesamtVerfuegbar}.");

                var verbleibend = command.MengeInBasiseinheiten;
                foreach (var charge in geordnet)
                {
                    if (verbleibend <= 0) break;

                    if (charge.Menge <= verbleibend)
                    {
                        verbleibend -= charge.Menge;
                        charge.Ausbuchen(command.AusgebuchtVonUserId, heute);
                    }
                    else
                    {
                        charge.MengeAnpassen(charge.Menge - verbleibend);
                        verbleibend = 0;
                    }
                }
            }

            await _chargeRepo.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var bestand = await _bestandRepo.GetByArtikelUndLagerortAsync(
                command.ArtikelId, command.Lagerort, cancellationToken);

            if (bestand is null || bestand.Menge < command.MengeInBasiseinheiten)
                return (false, $"Nicht genügend Bestand. Verfügbar: {bestand?.Menge ?? 0}.");

            bestand.Ausbuchen(command.MengeInBasiseinheiten);
            await _bestandRepo.SaveChangesAsync(cancellationToken);
        }

        return (true, null);
    }
}
