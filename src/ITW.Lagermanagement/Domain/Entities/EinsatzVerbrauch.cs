using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Domain.Entities;

public sealed class EinsatzVerbrauch
{
    private EinsatzVerbrauch()
    {
        ErstelltVonUserId = string.Empty;
        _positionen       = [];
    }

    public EinsatzVerbrauch(
        Guid id,
        DateOnly datum,
        Lagerort fahrzeug,
        int patienten,
        string? bemerkung,
        string erstelltVonUserId,
        DateTimeOffset erstelltAm)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id darf nicht leer sein.", nameof(id));
        if (fahrzeug == Lagerort.Depot)
            throw new ArgumentException("Ein Einsatzverbrauch muss einem Fahrzeug zugeordnet sein.", nameof(fahrzeug));
        if (patienten < 0)
            throw new ArgumentOutOfRangeException(nameof(patienten), "Patientenanzahl darf nicht negativ sein.");
        if (string.IsNullOrWhiteSpace(erstelltVonUserId))
            throw new ArgumentException("ErstelltVonUserId ist erforderlich.", nameof(erstelltVonUserId));

        Id                = id;
        Datum             = datum;
        Fahrzeug          = fahrzeug;
        Patienten         = patienten;
        Bemerkung         = string.IsNullOrWhiteSpace(bemerkung) ? null : bemerkung.Trim();
        ErstelltVonUserId = erstelltVonUserId.Trim();
        ErstelltAm        = erstelltAm;
        _positionen       = [];
    }

    private readonly List<EinsatzVerbrauchPosition> _positionen;

    public Guid          Id                { get; private set; }
    public DateOnly      Datum             { get; private set; }
    public Lagerort      Fahrzeug          { get; private set; }
    public int           Patienten         { get; private set; }
    public string?       Bemerkung         { get; private set; }
    public DateTimeOffset ErstelltAm       { get; private set; }
    public string        ErstelltVonUserId { get; private set; }

    public IReadOnlyList<EinsatzVerbrauchPosition> Positionen => _positionen.AsReadOnly();

    public void AddPosition(EinsatzVerbrauchPosition position)
    {
        ArgumentNullException.ThrowIfNull(position);
        _positionen.Add(position);
    }
}
