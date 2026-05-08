using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Domain.Entities;

public sealed class ArtikelBestand
{
    private ArtikelBestand() { }

    public ArtikelBestand(Guid id, Guid artikelId, Lagerort lagerort)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id darf nicht leer sein.", nameof(id));
        if (artikelId == Guid.Empty)
            throw new ArgumentException("ArtikelId darf nicht leer sein.", nameof(artikelId));

        Id        = id;
        ArtikelId = artikelId;
        Lagerort  = lagerort;
        Menge     = 0;
    }

    public Guid     Id        { get; private set; }
    public Guid     ArtikelId { get; private set; }
    public Lagerort Lagerort  { get; private set; }
    public int      Menge     { get; private set; }

    public void Einbuchen(int menge)
    {
        if (menge <= 0)
            throw new ArgumentOutOfRangeException(nameof(menge), "Einzubuchende Menge muss größer als 0 sein.");

        Menge += menge;
    }

    public void Ausbuchen(int menge)
    {
        if (menge <= 0)
            throw new ArgumentOutOfRangeException(nameof(menge), "Auszubuchende Menge muss größer als 0 sein.");
        if (menge > Menge)
            throw new InvalidOperationException(
                $"Nicht genügend Bestand. Verfügbar: {Menge}, angefragt: {menge}.");

        Menge -= menge;
    }
}
