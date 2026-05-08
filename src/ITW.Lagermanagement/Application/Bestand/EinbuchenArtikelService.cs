using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Application.Bestand;

public sealed record EinbuchenArtikelCommand(
    Guid ArtikelId,
    Lagerort Lagerort,
    int MengeInBasiseinheiten,
    string EingebuchtVonUserId,
    // Nur für Medikamente mit Ablaufdatum:
    DateOnly? Ablaufdatum = null,
    string? ChargeNummer = null);

public sealed class EinbuchenArtikelService
{
    private readonly ILagerArtikelRepository  _artikelRepo;
    private readonly IArtikelBestandRepository _bestandRepo;
    private readonly IArtikelChargeRepository  _chargeRepo;

    public EinbuchenArtikelService(
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
        EinbuchenArtikelCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.MengeInBasiseinheiten <= 0)
            return (false, "Menge muss größer als 0 sein.");

        var artikel = await _artikelRepo.GetByIdAsync(command.ArtikelId, cancellationToken);
        if (artikel is null)
            return (false, "Artikel wurde nicht gefunden.");

        if (!artikel.IstAktiv)
            return (false, "Deaktivierte Artikel können nicht eingebucht werden.");

        if (artikel.HatAblaufdatum)
        {
            if (!command.Ablaufdatum.HasValue)
                return (false, "Für diesen Artikel ist ein Ablaufdatum erforderlich.");

            var charge = new ArtikelCharge(
                Guid.NewGuid(),
                command.ArtikelId,
                command.Lagerort,
                command.MengeInBasiseinheiten,
                command.Ablaufdatum.Value,
                command.ChargeNummer,
                DateOnly.FromDateTime(DateTime.Today),
                command.EingebuchtVonUserId);

            await _chargeRepo.AddAsync(charge, cancellationToken);
            await _chargeRepo.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var bestand = await _bestandRepo.GetByArtikelUndLagerortAsync(
                command.ArtikelId, command.Lagerort, cancellationToken);

            if (bestand is null)
            {
                bestand = new ArtikelBestand(Guid.NewGuid(), command.ArtikelId, command.Lagerort);
                await _bestandRepo.AddAsync(bestand, cancellationToken);
            }

            bestand.Einbuchen(command.MengeInBasiseinheiten);
            await _bestandRepo.SaveChangesAsync(cancellationToken);
        }

        return (true, null);
    }
}
