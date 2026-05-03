<!-- Datei: docs/03_MVP/02_Erstes_MVP.md -->
# Erstes MVP der ITW-Suite

## Zweck dieses Dokuments

Dieses Dokument definiert den Umfang des ersten MVP der neuen ITW-Suite.

Das MVP soll klein genug sein, um sauber umgesetzt werden zu können, aber groß genug, um die Grundarchitektur praktisch zu validieren.

---

## 1. Ziel des ersten MVP

Das erste MVP soll beweisen, dass die neue Architektur funktioniert.

Insbesondere soll es zeigen, dass:

- zentrale Identity sauber funktioniert,
- Bereiche sauber getrennt sind,
- Rollen und Sichtbarkeiten korrekt greifen,
- Benutzerverwaltung zentral bleibt,
- ITW-spezifische Zusatzdaten sauber getrennt sind,
- Kontosicherheit zentral bleibt und trotzdem bereichsbezogen bearbeitet werden kann,
- Dienstwünsche korrekt im Modul Dienstplan aufgehoben sind.

---

## 2. Enthalten im MVP

## Plattform
- Login / Anmeldung
- zentrales Benutzerkonto
- Bereichszuordnung
- Bereichsrollen
- serverseitige Sichtbarkeiten
- einfache Auditierung
- Grundstruktur für Benachrichtigungen
- `Passwort vergessen`
- bereichsbezogene Passwort-Reset-Anfragen
- temporäres Passwort durch zuständige Leitung
- Pflicht zur Passwortänderung
  - nach initialem Passwort
  - nach temporärem Passwort

## Web
- Areas:
  - `Intensivtransport`
  - `Verwaltung`
  - `Geschaeftsfuehrung`
- area-spezifische Layouts auf gemeinsamer Shell-Basis
- gemeinsame Head-/Script-Ressourcen im Web-Projekt
- zentrales UI-Designsystem im Web-Projekt
- bereichsabhängige Navigation
- Dashboard-Grundseiten
- Badge-Anzeige für offene Passwort-Reset-Anfragen

## Zentrale Benutzerverwaltung
- Benutzer anlegen
- Benutzer aktivieren / sperren
- Benutzer einem Bereich zuordnen
- Bereichsrolle vergeben
- bereichsabhängige Benutzerlisten

## ITW-Personal
- ITW-Mitarbeiterprofil
- Qualifikationspflege
- Arzt / Notfallsanitäter
- allgemeines Mitarbeiterprofil / Stammdaten
- gebündelte Mitarbeiterdetailseite
- Mitarbeiterdokumente
- Dokumentkategorien für personalbezogene Unterlagen
- Upload / Download von Mitarbeiterdokumenten

## Dienstplan – erste Stufe
- Planungsperiode anlegen
- offene Periode anzeigen
- Wunschphase öffnen / schließen
- eigene Dienstwünsche abgeben
- gewünschte Dienstanzahl angeben
- eigene offene Perioden sehen

---

## 3. Nicht enthalten im ersten MVP

Folgende Themen gehören ausdrücklich **nicht** in das erste MVP:

- automatische Dienstplan-Generierung
- komplexe Verteil- und Fairnesslogik
- vollständige Einsatzverwaltung
- umfangreiche Auswertungen
- technische Spezialadministration
- komplexes Benachrichtigungssystem
- mehrstufige Freigabeprozesse
- umfangreiche Archivierungslogik
- ausgebautes Dokumentenmanagement mit Versionierung, Freigabe- oder Löschworkflow
- umfangreiche Nachweislogik über den grundlegenden Mitarbeiterdokumente-Bereich hinaus

---

## 4. Rollen im ersten MVP

Das erste MVP berücksichtigt folgende Rollen:

- ITW-Mitarbeiter
- Wachleiter
- Verwaltungsmitarbeiter
- Geschäftsführer Verwaltung
- Geschäftsführung
- technischer Administrator

---

## 5. Mindestanforderungen an das MVP

Das MVP ist nur dann fachlich erfolgreich, wenn folgende Mindestanforderungen erfüllt sind:

### Bereichstrennung
- Wachleiter sieht nur Benutzer aus Intensivtransport
- Geschäftsführer Verwaltung sieht nur Benutzer aus Verwaltung
- Bereichsleiter sehen keine fremden Benutzerbereiche

### Zentrale Benutzerverwaltung
- Benutzerkonten werden zentral geführt
- bereichsspezifische Sichten bleiben trotzdem getrennt

### Kontosicherheit
- Passwort-Reset bleibt zentral
- offene Reset-Anfragen werden nur im zuständigen Bereich sichtbar
- Benutzer mit initialem oder temporärem Passwort müssen dieses beim nächsten Login selbst ändern

### ITW-Personal
- ITW-spezifische Daten sind von Identity getrennt
- Qualifikationen sind nicht als Rollen modelliert
- Mitarbeiterdokumente sind an die bestehende Mitarbeiterdetailseite angedockt
- Dokumentkategorien sind fachlich vom technischen Dateinamen getrennt

### Dienstplan / Dienstwünsche
- Dienstwünsche gehören fachlich und technisch zum Modul Dienstplan
- Mitarbeiter können eigene Wünsche nur in offenen Perioden abgeben
- Wachleiter kann Perioden steuern

---

## 6. Architekturanforderungen an das MVP

Auch im MVP gelten bereits alle Architekturregeln:

- kein eigener Bereich für Dienstwünsche
- keine Fachlogik im Web-Projekt
- keine Vermischung von Identity und Fachdaten
- keine Bereichstrennung nur über Menüs
- keine unkontrollierten Querabhängigkeiten
- Mitarbeiterdokumente bleiben Teil des bestehenden Personalbereichs und werden nicht als neues Modul daneben gebaut

---

## 7. Erfolgskriterien

Das MVP ist erfolgreich, wenn:

1. die neue Projektstruktur tragfähig ist,
2. Bereiche sauber voneinander getrennt sind,
3. Benutzerverwaltung zentral bleibt,
4. ITW-Personal logisch getrennt geführt wird,
5. Passwort-Reset und Passwortwechselpflicht zentral und bereichsbezogen funktionieren,
6. Dienstwunsch sauber als Teil von Dienstplan umgesetzt ist,
7. spätere Module ohne Strukturbruch ergänzt werden können.

---

## 8. MVP-Abgrenzung

Das erste MVP ist bewusst kein vollständiges Zielsystem.

Es ist ein Architektur- und Grundlagen-MVP, kein Endausbau.

Es soll die Grundlage schaffen, auf der später folgende Ausbaustufen aufsetzen können:

- vollständiger Dienstplan
- Einsatzmodul
- erweiterte Auswertungen
- zentrale Benachrichtigungen
- zusätzliche Fachmodule

---

## 9. Zusammenfassung

Das erste MVP konzentriert sich auf das Fundament:

- Plattform
- Bereichstrennung
- Benutzerverwaltung
- Kontosicherheit / Passwort-Reset-Workflow
- ITW-Personal inklusive Mitarbeiterdokumente
- Dienstplan-Grundlage mit Dienstwünschen

Alles andere wird bewusst erst in späteren Phasen ergänzt.