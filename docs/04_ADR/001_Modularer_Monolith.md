# ADR 001 – Entscheidung für einen modularen Monolithen

## Status
Akzeptiert

## Datum
Wird beim ersten Commit des Architekturstands ergänzt.

---

## Kontext

Für die neue ITW-Suite wird eine zentrale Webanwendung aufgebaut, die mehrere organisatorische Bereiche in einem System vereint:

- Intensivtransport
- Verwaltung
- Geschäftsführung

Zusätzlich soll das System langfristig um neue Fachmodule erweitert werden können, insbesondere:

- Benutzerverwaltung
- ITW-Personal
- Dienstplan
- Einsätze
- weitere spätere Fachmodule

Wichtige Rahmenbedingungen:

- eine zentrale Identity-Basis
- strikte Bereichstrennung
- serverseitige Rechte- und Sichtbarkeitsprüfung
- langfristige Erweiterbarkeit
- praxisnahe Umsetzbarkeit in ASP.NET Core
- kein unnötiger technischer Overhead zu Beginn

---

## Entscheidung

Die ITW-Suite wird als **modularer Monolith** umgesetzt.

Das bedeutet konkret:

- eine Solution,
- eine zentrale Webanwendung als Host,
- mehrere fachlich getrennte Projekte / Module,
- klare Ownership pro Projekt,
- definierte Referenzrichtungen,
- keine Microservice-Aufteilung im Startsystem.

Die Ziel-Projektstruktur lautet:

- `ITW.Web`
- `ITW.Application`
- `ITW.Domain`
- `ITW.Infrastructure`
- `ITW.Dienstplan`
- `ITW.Einsatz`

---

## Begründung

Diese Entscheidung wurde getroffen, weil ein modularer Monolith für das Vorhaben die beste Balance aus Klarheit, Erweiterbarkeit und Umsetzbarkeit bietet.

### Vorteile
- zentrale Identity lässt sich sauber integrieren
- Rechte und Sichtbarkeiten bleiben zentral kontrollierbar
- Bereiche können in einer Anwendung getrennt dargestellt werden
- fachliche Module bleiben sauber abgrenzbar
- Deployment und Betrieb bleiben beherrschbar
- die Architektur ist mit ASP.NET Core gut umsetzbar
- spätere Erweiterungen sind möglich, ohne sofort verteilte Systeme einzuführen

### Vermeidete Nachteile anderer Ansätze
- kein verfrühter Microservice-Aufwand
- keine unnötig komplexe verteilte Kommunikation
- keine übergroße Ein-Projekt-Struktur ohne klare Grenzen
- keine frühe Aufsplitterung in künstliche Teilprojekte ohne fachlichen Bedarf

---

## Konsequenzen

### Positive Konsequenzen
- zentrale Plattformthemen bleiben zentral
- Fachmodule können sauber wachsen
- Bereiche und Module lassen sich getrennt denken
- Web, Plattform und Fachlogik bleiben besser strukturierbar
- Dienstwünsche können sauber als Teil von `ITW.Dienstplan` verbleiben

### Bewusste Konsequenzen
- Modulgrenzen müssen diszipliniert eingehalten werden
- Fachlogik darf nicht ins Web-Projekt ausweichen
- zentrale Projekte dürfen nicht zum Sammelbecken für alles werden
- Bereichsrechte müssen immer serverseitig geprüft werden

---

## Abgelehnte Alternativen

### 1. Klassischer großer Monolith ohne klare Modulgrenzen
Abgelehnt, weil:
- Verantwortlichkeiten schnell verschwimmen,
- Fachlogik leichter vermischt wird,
- spätere Erweiterungen schwieriger werden.

### 2. Microservices von Anfang an
Abgelehnt, weil:
- die Komplexität für den Start unnötig hoch wäre,
- Betrieb, Authentifizierung und Kommunikation deutlich aufwendiger würden,
- der aktuelle fachliche Zuschnitt noch keinen verteilten Systemzuschnitt erzwingt.

### 3. Eigenes Projekt für Dienstwünsche
Abgelehnt, weil:
- Dienstwünsche fachlich Teil des Dienstplanprozesses sind,
- Perioden, Wunschphase, Wunschabgabe und Planung einen gemeinsamen Lebenszyklus bilden,
- eine Trennung künstliche Kopplung und Doppelstrukturen erzeugen würde.

---

## Zusätzliche Festlegungen

Im Rahmen dieser Architekturentscheidung gelten zusätzlich folgende Festlegungen:

- Dienstwünsche bleiben Teil von `ITW.Dienstplan`
- `ITW.Infrastructure` darf `ITW.Web` nicht kennen
- Qualifikationen sind keine Rollen
- Geschäftsführung ist nicht automatisch technischer Volladmin
- Bereichstrennung wird serverseitig durchgesetzt
- Web-Areas strukturieren die Oberfläche, besitzen aber nicht die Fachlogik

---

## Überprüfung

Diese Entscheidung ist gültig, solange:

- die Lösung als gemeinsame Webanwendung betrieben wird,
- die Anzahl der Fachmodule im Rahmen eines modularen Monolithen gut beherrschbar bleibt,
- keine betrieblichen oder fachlichen Gründe eine verteilte Architektur erzwingen.

Eine spätere Neubewertung ist möglich, wenn:
- fachliche Module organisatorisch deutlich unabhängiger werden,
- Betrieb oder Skalierung eine andere Architektur erzwingen,
- Integrationsanforderungen grundlegend steigen.

---

## Zusammenfassung

Für die ITW-Suite ist der modulare Monolith die passende Zielarchitektur.

Er ermöglicht:
- zentrale Identity,
- saubere Bereichstrennung,
- klare Modulgrenzen,
- realistische Umsetzbarkeit,
- langfristige Erweiterbarkeit ohne unnötige Anfangskomplexität.