// Datei: src/ITW.Domain/Security/Entities/PasswortResetAnfrage.cs
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Security.Enums;

namespace ITW.Domain.Security.Entities;

public sealed class PasswortResetAnfrage
{
    private PasswortResetAnfrage()
    {
        UserId = string.Empty;
        Benutzername = string.Empty;
        Vorname = string.Empty;
        Nachname = string.Empty;
        BearbeitetVonUserId = null;
    }

    public PasswortResetAnfrage(
        Guid id,
        string userId,
        string benutzername,
        string vorname,
        string nachname,
        Organisationsbereich bereich,
        DateTimeOffset angefordertAm)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID der Passwort-Reset-Anfrage darf nicht leer sein.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Die UserId ist erforderlich.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(benutzername))
        {
            throw new ArgumentException("Der Benutzername ist erforderlich.", nameof(benutzername));
        }

        if (string.IsNullOrWhiteSpace(vorname))
        {
            throw new ArgumentException("Der Vorname ist erforderlich.", nameof(vorname));
        }

        if (string.IsNullOrWhiteSpace(nachname))
        {
            throw new ArgumentException("Der Nachname ist erforderlich.", nameof(nachname));
        }

        if (bereich == Organisationsbereich.Unbekannt)
        {
            throw new ArgumentException("Der Organisationsbereich ist ungültig.", nameof(bereich));
        }

        Id = id;
        UserId = userId.Trim();
        Benutzername = benutzername.Trim();
        Vorname = vorname.Trim();
        Nachname = nachname.Trim();
        Bereich = bereich;
        Status = PasswortResetAnfrageStatus.Offen;
        AngefordertAm = angefordertAm;
    }

    public Guid Id { get; private set; }

    public string UserId { get; private set; }

    public string Benutzername { get; private set; }

    public string Vorname { get; private set; }

    public string Nachname { get; private set; }

    public Organisationsbereich Bereich { get; private set; }

    public PasswortResetAnfrageStatus Status { get; private set; }

    public DateTimeOffset AngefordertAm { get; private set; }

    public DateTimeOffset? BearbeitetAm { get; private set; }

    public string? BearbeitetVonUserId { get; private set; }

    public void AlsErledigtMarkieren(
        string bearbeitetVonUserId,
        DateTimeOffset bearbeitetAm)
    {
        if (string.IsNullOrWhiteSpace(bearbeitetVonUserId))
        {
            throw new ArgumentException("Die Bearbeiter-UserId ist erforderlich.", nameof(bearbeitetVonUserId));
        }

        if (Status != PasswortResetAnfrageStatus.Offen)
        {
            throw new InvalidOperationException("Nur offene Passwort-Reset-Anfragen können erledigt werden.");
        }

        Status = PasswortResetAnfrageStatus.Erledigt;
        BearbeitetAm = bearbeitetAm;
        BearbeitetVonUserId = bearbeitetVonUserId.Trim();
    }

    public void AlsAbgelehntMarkieren(
        string bearbeitetVonUserId,
        DateTimeOffset bearbeitetAm)
    {
        if (string.IsNullOrWhiteSpace(bearbeitetVonUserId))
        {
            throw new ArgumentException("Die Bearbeiter-UserId ist erforderlich.", nameof(bearbeitetVonUserId));
        }

        if (Status != PasswortResetAnfrageStatus.Offen)
        {
            throw new InvalidOperationException("Nur offene Passwort-Reset-Anfragen können abgelehnt werden.");
        }

        Status = PasswortResetAnfrageStatus.Abgelehnt;
        BearbeitetAm = bearbeitetAm;
        BearbeitetVonUserId = bearbeitetVonUserId.Trim();
    }
}