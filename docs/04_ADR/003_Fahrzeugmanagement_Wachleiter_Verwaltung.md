# ADR 003: Fahrzeugmanagement zwischen Wachleiter, Verwaltung und Geschäftsführung trennen

## Status

Angenommen

---

## Kontext

Das Fahrzeugmanagement der ITW-Suite wurde zunächst im Bereich Intensivtransport aufgebaut.

Im Verlauf der fachlichen Klärung wurde deutlich, dass Fahrzeugmanagement nicht nur eine Wachleiterfunktion ist.

Es gibt unterschiedliche fachliche Perspektiven:

```text
Intensivtransport / Wachleiter:
operative Fahrzeugakte

Verwaltung:
administrative Fahrzeugverwaltung

Geschäftsführung:
spätere Auswertungen und Kostenübersicht
```

Diese Perspektiven dürfen nicht vermischt werden.

Der Wachleiter braucht eine einfache, schnelle und operative Sicht.

Die Verwaltung benötigt später Verträge, Policen, Rechnungen, Leasing, Finanzierung und allgemeine Fahrzeugdokumente.

Die Geschäftsführung soll später übergreifend Kosten und Auswertungen sehen können.

---

## Entscheidung

Das Fahrzeugmanagement wird fachlich getrennt geführt.

---

## Intensivtransport / Wachleiter

Der Wachleiter erhält eine operative Fahrzeugakte.

Diese enthält:

- Fahrzeugübersicht
- Stammdaten
- Fahrtenbuch
- Tankbelege
- Prüfstatus

Diese enthält nicht:

- Verträge
- Versicherungspolicen
- Leasing
- Finanzierung
- Werkstattrechnungen
- allgemeine Fahrzeugdokumente
- Kostenberichte
- Buchhaltung
- MPG-Verwaltung
- Verwaltungskalender
- separate Fahrzeugstandort-Seite

Der Fahrzeugstandort wird nicht mehr als eigener Menüpunkt im Fahrzeugmanagement geführt.

Später soll der Standort einsatzbezogen angezeigt werden, z. B. als kleine Karte in der Einsatzansicht.

---

## Verwaltung

Die Verwaltung erhält später die administrative Fahrzeugverwaltung.

Dort gehören hin:

- Verträge
- Versicherungspolicen
- Leasing
- Finanzierung
- Werkstattrechnungen
- allgemeine Fahrzeugdokumente
- Kosteninformationen
- Abrechnungsdaten

Diese Inhalte werden nicht in den Wachleiterbereich zurückgebaut.

---

## Geschäftsführung

Die Geschäftsführung erhält später Auswertungen, sofern die passenden Module freigegeben sind.

Beispiele:

- Fahrzeugkosten
- Einsatzkosten
- Materialkosten
- Diesel-/Tankkosten
- übergreifende Kostenentwicklung

Die Geschäftsführung ist nicht die operative Erfassungsstelle.

---

## Begründung

Der Wachleiter soll im Einsatzalltag nicht mit Verwaltungs- und Kostenlogik überladen werden.

Ein operatives Fahrzeugmanagement muss schnell nutzbar sein.

Der Wachleiter muss erkennen:

- welches Fahrzeug einsatzbereit ist,
- welcher Prüfstatus besteht,
- welche Tankbelege vorhanden sind,
- welche Fahrtenbuchdaten erfasst wurden,
- welcher Status gepflegt werden muss.

Verträge, Rechnungen und Policen gehören fachlich nicht in diesen Ablauf.

---

## Fahrtenbuch-Entscheidung

Das Fahrtenbuch wird im Wachleiterbereich vereinfacht.

Es gibt keinen Zwei-Schritt-Ablauf mehr:

```text
Fahrt anlegen -> später abschließen
```

Stattdessen wird ein Fahrtenbucheintrag einmal vollständig gespeichert.

Enthalten:

- Kategorie
- Fahrtzweck
- Startzeit
- Endzeit
- Fahrer
- Beifahrer
- Von
- Nach
- Startkilometerstand
- Endkilometerstand
- Bemerkung

Nicht enthalten:

- Tankmenge
- Kilometerstand beim Tanken
- Kraftstoffkosten
- Einsatzkosten

Tankdaten sollen später über Einsatzdokumentation bzw. Tablet-Erfassung entstehen.

Langfristig kann `ITW.Einsatz` Fahrtenbuchdaten automatisch oder teilautomatisch vorbereiten.

---

## Tankbelege-Entscheidung

Im Wachleiterbereich heißt die Funktion fachlich `Tankbelege`.

Technisch darf die vorhandene Fahrzeugdokument-Logik genutzt werden.

Regeln:

- Wachleiter wählt keine allgemeine Dokumentenkategorie.
- Server-seitig wird immer `FahrzeugDokumentKategorie.Tankbeleg` gespeichert.
- Tankbelege benötigen eine Bezeichnung.
- Tankbelege werden nach Upload-Datum sortiert, neueste zuerst.
- Dateien liegen im Server-Dateisystem.
- Die Datenbank speichert nur Metadaten und Speicherpfad.
- Keine Datei-Inhalte / Blobs in der Datenbank.

Ablage:

```text
App_Data/Fahrzeugdokumente/{FahrzeugId}/{Guid}.{endung}
```

Allgemeine Fahrzeugdokumente gehören später in die Verwaltung.

---

## Prüfstatus-Entscheidung

Der Prüfstatus ist im Wachleiterbereich eine einfache Ampelanzeige.

Kein Terminkalender.

Prüfpunkte:

- HU/AU
- Sicherheitsprüfung elektrische Anlage
- Sicherheitsprüfung Sauerstoffanlage
- Sicherheitsprüfung Aufbau
- Service allgemein

Nicht enthalten:

- MPG
- Verträge
- Rechnungen
- Werkstattkalender
- Betreiberkalender
- Uhrzeiten

Ampellogik:

- Grün = mehr als 30 Tage Zeit
- Gelb = innerhalb von 30 Tagen fällig
- Rot = überfällig
- Grau = nicht hinterlegt

---

## Fahrzeugcheck und Mängel

Fahrzeugcheck und Mängelerfassung gehören später fachlich in die Tablet-App bzw. auf das Einsatztablet der Besatzung.

Zielrichtung:

```text
Besatzung im Einsatztablet
-> Fahrzeugcheck vor Dienstbeginn
-> Mängel erfassen
-> Wachleiter sieht offene Mängel
```

Der Wachleiter soll später:

- offene Mängel sehen,
- Einsatzfähigkeit bewerten,
- Fahrzeugstatus setzen,
- Maßnahmen veranlassen.

Die Checkliste selbst gehört jedoch in die Hände der Besatzung.

---

## Kosten und spätere Auswertung

Später soll nachvollziehbar sein:

- welcher Einsatz welche Materialkosten verursacht hat,
- welcher Einsatz welche Diesel-/Tankkosten verursacht hat,
- welche weiteren einsatzbezogenen Kosten angefallen sind,
- welche Gesamtkosten ein Fahrzeug verursacht.

Diese Funktionen gehören nicht in die operative Wachleiterakte.

Zuständigkeit:

```text
Verwaltung:
Pflege und Verwaltung administrativer Kosten- und Dokumentendaten

Geschäftsführung:
Auswertung und Übersicht

Einsatzverwaltung:
spätere einsatzbezogene Zuordnung

Materialmanagement:
späterer Materialverbrauch
```

Nur sinnvoll bei passenden Modulfreigaben:

- Einsatzverwaltung
- Fahrzeugmanagement
- Materialmanagement

Jetzt wird kein großes Kostenmodul gebaut.

Die vorhandenen Strukturen sollen spätere Verknüpfungen aber nicht blockieren.

---

## Technische Konsequenzen

Es gilt:

- bestehende Struktur beibehalten
- kein neues Projekt nur für Verwaltung-Fahrzeugmanagement
- keine künstliche neue Architektur
- keine Schichtvermischung
- keine Logik duplizieren
- Web-Orchestrierung und ViewModels bleiben im Web-Projekt
- Fachlogik bleibt in `ITW.Fahrzeugmanagement`
- EF Core / Datenzugriff bleibt in Infrastructure
- `ITW.Infrastructure` darf `ITW.Web` nicht kennen

Wichtig:

Domain-Entities und Application-Services für Verträge dürfen bestehen bleiben, auch wenn sie im Wachleiterbereich nicht mehr sichtbar sind.

Sie können später durch die Verwaltung genutzt werden.

---

## UI-Konsequenzen

Für das Wachleiter-Fahrzeugmanagement gilt:

- einfache operative Oberfläche
- moderne Fahrzeugübersicht
- schnelle Aktionen
- keine Verwaltungsüberladung
- keine Inline-Styles
- keine Inline-Scripts
- keine View-spezifischen CSS-Dateien
- Modulstyles liegen in `ITW.Fahrzeugmanagement.css`

---

## Verworfene Alternative

Verworfen wurde:

```text
Ein großes Fahrzeugmanagement für alle Rollen in einer gemeinsamen Oberfläche.
```

Grund:

- Wachleiter würde mit Verwaltungsdaten überladen.
- Verwaltung würde operative Sonderlogik sehen.
- spätere Geschäftsführungsauswertungen würden mit Erfassung vermischt.
- Zuständigkeiten wären unklar.
- UI würde schnell unübersichtlich.

---

## Merksatz

Der Wachleiter führt die operative Fahrzeugakte.  
Die Verwaltung führt die administrative Fahrzeugverwaltung.  
Die Geschäftsführung wertet später aus.