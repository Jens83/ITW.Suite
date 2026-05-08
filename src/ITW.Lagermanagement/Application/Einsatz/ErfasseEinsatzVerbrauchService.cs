using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Application.Einsatz;

public sealed record EinsatzVerbrauchArtikelCommand(
    Guid ArtikelId,
    int Menge,
    bool IstProPatientBerechnet);

public sealed record ErfasseEinsatzVerbrauchCommand(
    DateOnly Datum,
    Lagerort Fahrzeug,
    int Patienten,
    string? Bemerkung,
    IReadOnlyList<EinsatzVerbrauchArtikelCommand> ManuellePositionen,
    string ErstelltVonUserId);

public sealed class ErfasseEinsatzVerbrauchResult
{
    private ErfasseEinsatzVerbrauchResult(bool isSuccess, string? errorMessage, Guid verbrauchId = default)
    {
        IsSuccess    = isSuccess;
        ErrorMessage = errorMessage;
        VerbrauchId  = verbrauchId;
    }

    public bool    IsSuccess    { get; }
    public string? ErrorMessage { get; }
    public Guid    VerbrauchId  { get; }

    public static ErfasseEinsatzVerbrauchResult Erfolg(Guid id) => new(true, null, id);
    public static ErfasseEinsatzVerbrauchResult Fehler(string msg) => new(false, msg);
}

public sealed class ErfasseEinsatzVerbrauchService
{
    private readonly ILagerArtikelRepository   _artikelRepo;
    private readonly IEinsatzVerbrauchRepository _verbrauchRepo;
    private readonly IArtikelBestandRepository  _bestandRepo;

    public ErfasseEinsatzVerbrauchService(
        ILagerArtikelRepository artikelRepo,
        IEinsatzVerbrauchRepository verbrauchRepo,
        IArtikelBestandRepository bestandRepo)
    {
        ArgumentNullException.ThrowIfNull(artikelRepo);
        ArgumentNullException.ThrowIfNull(verbrauchRepo);
        ArgumentNullException.ThrowIfNull(bestandRepo);
        _artikelRepo   = artikelRepo;
        _verbrauchRepo = verbrauchRepo;
        _bestandRepo   = bestandRepo;
    }

    public async Task<ErfasseEinsatzVerbrauchResult> ExecuteAsync(
        ErfasseEinsatzVerbrauchCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Fahrzeug == Lagerort.Depot)
            return ErfasseEinsatzVerbrauchResult.Fehler("Einsatzverbrauch muss einem Fahrzeug zugeordnet sein.");

        if (command.Patienten < 0)
            return ErfasseEinsatzVerbrauchResult.Fehler("Patientenanzahl darf nicht negativ sein.");

        var verbrauch = new EinsatzVerbrauch(
            Guid.NewGuid(),
            command.Datum,
            command.Fahrzeug,
            command.Patienten,
            command.Bemerkung,
            command.ErstelltVonUserId,
            DateTimeOffset.UtcNow);

        // Pro-Patient-Artikel automatisch berechnen
        if (command.Patienten > 0)
        {
            var alleArtikel = await _artikelRepo.GetAktiveAsync(cancellationToken);
            foreach (var artikel in alleArtikel.Where(a => a.VerbrauchProPatient.HasValue))
            {
                var berechnete = (int)Math.Ceiling(artikel.VerbrauchProPatient!.Value * command.Patienten);
                if (berechnete > 0)
                {
                    verbrauch.AddPosition(new EinsatzVerbrauchPosition(
                        Guid.NewGuid(),
                        verbrauch.Id,
                        artikel.Id,
                        berechnete,
                        istProPatientBerechnet: true));
                }
            }
        }

        // Manuelle Positionen (überschreiben Pro-Patient wenn doppelt)
        foreach (var pos in command.ManuellePositionen)
        {
            verbrauch.AddPosition(new EinsatzVerbrauchPosition(
                Guid.NewGuid(),
                verbrauch.Id,
                pos.ArtikelId,
                pos.Menge,
                istProPatientBerechnet: false));
        }

        await _verbrauchRepo.AddAsync(verbrauch, cancellationToken);
        await _verbrauchRepo.SaveChangesAsync(cancellationToken);

        return ErfasseEinsatzVerbrauchResult.Erfolg(verbrauch.Id);
    }
}
