namespace ITW.Lagermanagement.Domain.Entities;

public sealed class EinsatzVerbrauchPosition
{
    private EinsatzVerbrauchPosition() { }

    public EinsatzVerbrauchPosition(
        Guid id,
        Guid einsatzVerbrauchId,
        Guid artikelId,
        int menge,
        bool istProPatientBerechnet)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id darf nicht leer sein.", nameof(id));
        if (einsatzVerbrauchId == Guid.Empty)
            throw new ArgumentException("EinsatzVerbrauchId darf nicht leer sein.", nameof(einsatzVerbrauchId));
        if (artikelId == Guid.Empty)
            throw new ArgumentException("ArtikelId darf nicht leer sein.", nameof(artikelId));
        if (menge <= 0)
            throw new ArgumentOutOfRangeException(nameof(menge), "Menge muss größer als 0 sein.");

        Id                     = id;
        EinsatzVerbrauchId     = einsatzVerbrauchId;
        ArtikelId              = artikelId;
        Menge                  = menge;
        IstProPatientBerechnet = istProPatientBerechnet;
    }

    public Guid Id                     { get; private set; }
    public Guid EinsatzVerbrauchId     { get; private set; }
    public Guid ArtikelId              { get; private set; }
    public int  Menge                  { get; private set; }
    public bool IstProPatientBerechnet { get; private set; }
}
