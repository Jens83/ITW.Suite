# ADR 004 – Gemeinsame Persistenz über PlatformDbContext

## Status
Akzeptiert

## Datum
2026-05-05

---

## Kontext

Die ITW-Suite besteht aus mehreren Fachmodulen, die alle auf denselben relationalen Daten operieren:

- `ITW.Dienstplan` – Perioden, Dienste, Wünsche
- `ITW.Fahrzeugmanagement` – Fahrzeuge, Dokumente, Fahrtenbuch, Tracking
- `ITW.Einsatz` – geplant

Für die Persistenz stellt sich die Frage, ob jedes Modul seinen eigenen DbContext erhält oder ob ein gemeinsamer Kontext verwendet wird.

---

## Entscheidung

Die ITW-Suite verwendet **einen gemeinsamen `PlatformDbContext`** in `ITW.Infrastructure`.

Alle Fachmodule werden über diesen zentralen Kontext persistiert.

---

## Begründung

Ein gemeinsamer DbContext ist für den aktuellen Stand sinnvoll, weil:

- die Lösung ein modularer Monolith mit einer gemeinsamen Datenbank ist,
- die Anzahl der Fachmodule aktuell überschaubar bleibt,
- cross-modulare Abfragen (z. B. Dienstplan + Personal) einfacher bleiben,
- EF-Core-Migrationen zentral verwaltet werden können,
- der Betriebsaufwand geringer bleibt als bei mehreren Datenbanken.

---

## Konsequenzen

### Positive Konsequenzen
- eine Migration für alle Fachmodule
- gemeinsame Transaktionen möglich
- einfacheres Deployment

### Bewusste Konsequenzen
- `ITW.Infrastructure` kennt alle Fachmodul-Entities
- Modulgrenzen werden nicht durch Datenbankgrenzen erzwungen, sondern durch disziplinierte Codeorganisation
- Beim Wachstum ist eine Neubewertung erforderlich (siehe Split-Trigger)

---

## Auslöser für eine Neubewertung (Split-Trigger)

Diese Entscheidung wird neu bewertet, wenn **einer** der folgenden Trigger eintritt:

| Trigger | Schwellwert |
|---|---|
| Anzahl aktiver Fachmodule | ≥ 4 |
| Anzahl DbSets im PlatformDbContext | > 50 |
| Anzahl aktiver Entwickler | > 3 |
| Migrationskonflikte häufen sich | wiederkehrendes Problem |

Bei Eintreten eines Triggers: Evaluierung von modulspezifischen DbContexts oder separaten Datenbanken pro Fachmodul.

---

## Abgelehnte Alternativen

### Eigener DbContext pro Fachmodul
Abgelehnt für den aktuellen Stand, weil:
- cross-modulare Abfragen deutlich komplexer würden,
- Transaktionsmanagement aufwendiger würde,
- der Gewinn an Isolation den Aufwand für 2–3 Module nicht rechtfertigt.

---

## Zusammenfassung

Ein gemeinsamer `PlatformDbContext` ist für den modularen Monolith im aktuellen Wachstumsstadium der richtige Ansatz. Bei definierten Wachstumsschwellen wird die Entscheidung neu bewertet.
