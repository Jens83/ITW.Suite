# Umsetzungsstand der ITW-Suite

## 1. Zweck dieses Dokuments

Dieses Dokument beschreibt den aktuellen fachlichen und technischen Umsetzungsstand der ITW-Suite.

Es dient dazu:

- den aktuellen Stand nachvollziehbar festzuhalten,
- abgeschlossene Entscheidungen zu dokumentieren,
- laufende Refactorings einzuordnen,
- fachliche Zuständigkeiten festzuhalten,
- technische Grenzen zu sichern,
- die nächsten Schritte sauber auf dem echten Ist-Stand aufzubauen.

Wichtig:

Die ZIP kann älter sein als der lokale Entwicklungsstand aus den letzten Arbeitsschritten. Dieses Dokument beschreibt den aktuellen Ziel- und Arbeitsstand nach den zuletzt vorgenommenen fachlichen Korrekturen.

---

## 2. Grundsystem und Plattform

### 2.1 Zentrale Modulsteuerung

Vorhanden bzw. im Ausbau:

- Bereiche
- Bereichsrollen
- zentrale Modulgrundlage
- Modulzuweisungen als eigene zentrale Organisationslogik
- Laden aktiver Module anhand von Bereich und Rolle
- aktiver Benutzerkontext enthält die für Bereich und Rolle freigegebenen Module
- serverseitige Modulprüfung über Controller-/Zugriffslogik

Wichtige Festlegungen:

- Bereiche und Module sind nicht dasselbe.
- Module werden direkt über Bereich und Rolle freigegeben.
- Es gibt keine Mandantenlogik für Modulsichtbarkeit.
- Es gibt keine Kundenlogik für Modulsichtbarkeit.
- Es gibt keine Modulsichtbarkeit über `MandantId`.
- Modulfreigaben laufen direkt über Bereich und Rolle.
- Die Geschäftsführung legt zentral fest, welche Module es gibt und wer sie bekommt.
- Navigation ist nur Sichtbarkeit, nicht die Sicherheitsgrenze.
- Controller-Zugriffe müssen serverseitig geschützt sein.

Beispiele für Modulfreigaben:

- `Dienstplan` für `Intensivtransport / Mitarbeiter`
- `Dienstplan` für `Intensivtransport / Wachleiter`
- `Personal` für `Intensivtransport / Wachleiter`
- `Fahrzeugmanagement` für `Intensivtransport / Wachleiter`
- `Abrechnung` für `Verwaltung / Verwaltungsmitarbeiter`

---

### 2.2 Web-Grundsystem

Vorhanden bzw. weit fortgeschritten:

- bereichsspezifische Areas
- eigene Layouts je Bereich
- zentrale Shell
- gemeinsames Frontend-System
- zentrale Komponenten
- konsolidierte Dashboard- und Seitenstruktur
- zentrale Resource-Partials für Head und Scripts
- zentrale CSS-/JS-Struktur für Shell, Komponenten, Module und Themes

Zusätzlich im aktuellen technischen Stand:

- `Program.cs` ist entschlackt.
- Service-Registrierungen wurden in Web-DI-Extensions ausgelagert.
- Identity/Auth/Cookie-Konfiguration wurde in eigene Setup-Extensions ausgelagert.
- Startup-/Bootstrap-Initialisierung wurde in eine eigene Startup-Extension ausgelagert.
- Web-spezifische Dienstplan-Orchestrierungsservices wurden eingeführt, damit Controller schlank bleiben.
- Fahrzeugmanagement-Webfunktionen wurden schrittweise an die bestehende Web-Struktur angebunden.

Frontend-Regel:

- Views enthalten Struktur und Daten.
- Views enthalten keine Inline-Styles.
- Views enthalten keine Inline-Scripts.
- Zentrale Styles und Scripts werden über die vorhandenen App-Ressourcen eingebunden.
- Neue Sonder-CSS-Dateien pro View sollen vermieden werden.
- Vorhandene App-Klassen sollen konsequent genutzt werden.
- Statusmeldungen sollen über die gemeinsame Komponente `_AppStatusMessage.cshtml` laufen.

Genutzte zentrale UI-Klassen:

- `app-page`
- `app-page-header`
- `app-page-heading`
- `app-page-actions`
- `app-employee-card`
- `app-employee-detail-grid`
- `app-employee-doc-grid`
- `app-btn`
- Bootstrap-Badges
- Bootstrap-Formular- und Tabellenklassen

---

### 2.3 Zentrale Benutzer- und Organisationsverwaltung

Vorhanden:

- Bereichszuordnungen
- Benutzerlisten pro Bereich
- neue Benutzerkonten
- Zuordnung bestehender Konten
- Rollenänderung
- Sperren / Aktivieren

Wichtig im aktuellen Stand:

- Die Benutzerverwaltung wurde bewusst vereinfacht.
- Benutzer werden organisatorisch direkt einem Bereich und einer Bereichsrolle zugeordnet.
- Die spätere Modulfreigabe baut darauf auf.
- Bereichsbenutzer-Logik bleibt im Web-/Application-Kontext und wird nicht in einzelne Fachmodule dupliziert.

---

### 2.4 Modulare Sichtbarkeit

Aktueller Zielstand:

- Navigation, Dashboard-Kacheln und Controller-Zugriffe hängen an Bereich, Rolle und Modulzuweisung.
- Modulfreigaben laufen direkt über Bereich + Rolle.
- Keine Mandanten-/Kundenlogik für Module.
- Keine MandantId als Grundlage für Modulsichtbarkeit.

Beispiele:

```text
Dienstplan -> Intensivtransport / Mitarbeiter
Dienstplan -> Intensivtransport / Wachleiter
Personal -> Intensivtransport / Wachleiter
Fahrzeugmanagement -> Intensivtransport / Wachleiter
Fahrzeugmanagement -> Verwaltung / Verwaltungsmitarbeiter
Fahrzeugmanagement -> Verwaltung / Bereichsleitung
Abrechnung -> Verwaltung / Verwaltungsmitarbeiter
```

---

## 3. Dienstplan – aktueller technischer und fachlicher Stand

### 3.1 Grundstruktur

Vorhanden:

- Dienstplanperioden
- Wunschphasen
- Dienstwünsche
- gewünschte Dienstanzahl
- manuelle Planung
- veröffentlichter Plan
- Monatsauswertung
- Kalenderdarstellungen
- erste Autoplan-Funktionen

Wichtig:

- Dienstwünsche sind fachlich Teil des Moduls Dienstplan.
- Der Dienstplan bleibt ein eigenes Fachmodul.
- Mitarbeiter- und Wachleiterfluss bleiben getrennt.
- Die Modulfreigabe schaltet diese Flüsse frei oder aus, ersetzt sie aber nicht.

---

### 3.2 Mitarbeiterseite / Wunschabgabe

Vorhanden:

- offene Wunschphasen
- Wunschabgabe im Kalender
- Speicherung der Wünsche
- Freigabe-/Periodenbezug
- veröffentlichter Plan

Technisch aktueller Stand:

- eigener `DienstplanMitarbeiterController`
- Mitarbeiter-Index und veröffentlichter Plan sind getrennt vom Wachleiter-Teil
- Wunschspeicherung und Monatswunschspeicherung laufen über eigene Web-Orchestrierungsservices

---

### 3.3 Wachleiterflow / Perioden / Kalender

Vorhanden:

- Wachleiterkalender
- Periodensteuerung
- Wunschsicht
- Tages- und Monatsübersicht
- Freigabe und Veröffentlichung
- manuelle Eingriffe
- Konfliktdarstellungen
- Autoplan-Zugänge

Technisch aktueller Stand:

- eigener `DienstplanWachleiterController`
- Wachleiterkalender und Planungsmodal sind aus dem früheren Sammelcontroller herausgelöst
- Tagesplanung und Ausfallverarbeitung laufen über eigene Web-Orchestrierungsservices
- gemeinsame Bereichs-/Zugriffslogik wurde in eine gemeinsame Basisklasse verschoben

---

### 3.4 Auswertung

Vorhanden:

- Monatsauswertung
- Buchhaltungs-PDF

Technisch aktueller Stand:

- eigener `DienstplanAuswertungController`
- Auswertungs-ViewModel-Aufbereitung läuft über eigenen Read-Service
- PDF-Erstellung nutzt weiterhin das bestehende Dokument, wird aber nicht mehr vom großen Sammelcontroller getragen

---

### 3.5 Einstieg / Routing

Aktueller technischer Stand:

- `DienstplanController` ist nur noch Einstieg und Rollenweiterleitung
- die operative Fachlogik liegt nicht mehr in einem großen Sammelcontroller
- Mitarbeiter, Wachleiter und Auswertung haben jeweils eigene Controllerverantwortung
- Views und Dashboard-/Autoplan-/Urlaubsplaner-Verweise wurden auf die neuen Zielcontroller umgestellt

---

### 3.6 Web-Orchestrierungsservices im Dienstplan

Im aktuellen Stand wurden im Web-Projekt gezielt Orchestrierungsservices eingeführt, damit:

- Controller schlank bleiben,
- ViewModels nicht im Controller zusammengesetzt werden,
- Redirect-/Modal-/Ansichtslogik nicht in den Fachmodulen landet,
- die Schichtgrenzen erhalten bleiben.

Wichtig:

Diese Services sind Web-Koordination, nicht fachliche Kernlogik.

Sie bleiben deshalb im Web-Projekt.

---

### 3.7 Dienstplan-Fachregeln

Aktuelle fachliche Regeln:

- Änderungen am freigegebenen Dienstplan erfolgen nicht automatisiert im Programm.
- Änderungen erfolgen nur nach vorheriger telefonischer Abstimmung durch den Wachleiter mit den betroffenen Mitarbeitern.
- Jede vollzogene Änderung muss danach im Mitarbeiter-Dienstplan sichtbar sein.
- Änderungen müssen klar als Dienst oder Vertretung erscheinen.

Beschäftigungsarten:

- `Festangestellt`
- `Freelancer`
- `Honorarkraft`

Honorarkräfte:

- haben keinen Urlaubsanspruch
- erscheinen nicht im Urlaubsplaner
- dürfen Dienstwünsche abgeben
- sollen bei vorhandenen Wünschen in die automatische Planung einbezogen werden
- können ohne Wünsche nur nach Absprache manuell eingeplant werden

---

## 4. Personalmodul

Vorhanden bzw. im Ausbau:

- ITW-Mitarbeiterprofile
- Qualifikationen
- zentrale Mitarbeiterbasis
- erste konsolidierte Personalsichten
- Mitarbeiterdetailseite mit linker Navigation
- Dokumentenbereich bei Personaldaten
- Upload von Personaldokumenten nach vorhandenem Dateispeicher-Muster

Wichtig im Zielbild:

- Das Modul `Personal` kann getrennt vom Modul `Dienstplan` freigegeben werden.
- Dadurch kann der Wachleiter z. B. Dienstplan erhalten, ohne automatisch alle Personalfunktionen zu sehen.
- Ebenso kann `Personal` gezielt für bestimmte Rollen sichtbar sein.

Dokumente im Personalbereich:

- Dateien liegen serverseitig im `App_Data`-Bereich.
- Die Datenbank speichert nur Metadaten und Speicherpfad.
- Datei-Inhalte werden nicht in der Datenbank gespeichert.

Dieses Muster wurde später für Fahrzeug-Tankbelege übernommen.

---

## 5. Verwaltung

Vorhanden bzw. vorbereitet:

- eigener Bereich
- eigenes Layout
- eigene Benutzerverwaltung
- eigenes Dashboard
- Grundlagen für spätere kaufmännische Module

Zielrichtung:

- Verwaltung erhält eigene fachliche Module.
- Diese werden wie im Rest der Suite zentral über Bereich, Rolle und Modulzuweisung gesteuert.
- Verwaltungsinhalte werden nicht in den Wachleiterbereich gemischt.

Für das Fahrzeugmanagement bedeutet das:

In die Verwaltung gehören später:

- Verträge
- Versicherungspolicen
- Leasing
- Finanzierung
- Werkstattrechnungen
- allgemeine Fahrzeugdokumente
- Kosteninformationen
- Abrechnungsdaten

Diese Inhalte gehören nicht in die operative Fahrzeugakte des Wachleiters.

---

## 6. Fahrzeugmanagement – aktueller technischer und fachlicher Stand

### 6.1 Grundsatzentscheidung

Das Fahrzeugmanagement wurde begonnen und fachlich mehrfach korrigiert.

Der aktuelle Zuschnitt unterscheidet klar zwischen:

```text
Intensivtransport / Wachleiter:
operative Fahrzeugakte

Verwaltung:
administrative Fahrzeugverwaltung

Geschäftsführung:
spätere Auswertungen und Kostenübersicht
```

Wichtig:

- Das Fahrzeugmanagement ist nicht nur ein Wachleiter-Modul.
- Der Wachleiterbereich bleibt operativ und einfach.
- Verwaltungsfunktionen werden nicht in die Wachleiter-Fahrzeugakte zurückgebaut.
- Die Geschäftsführung bekommt später Auswertungen, aber keine operative Erfassungsmaske.

---

### 6.2 Vorhandene technische Grundlage

Im aktuellen Stand ist das Modul `ITW.Fahrzeugmanagement` bereits begonnen.

Vorhanden sind unter anderem Domain-Entities für:

- Fahrzeuge
- Fahrzeugdokumente
- Fahrzeugverträge
- Fahrtenbucheinträge
- Fahrzeugprüfungen
- Tracking-Geräte
- aktuelle Tracking-Geräte-Standorte
- historische Tracking-Geräte-Standorte
- Tracking-Geräte-Einrichtungscodes

Wichtig:

Die Vertrags-Entities und Vertragsservices dürfen bestehen bleiben.  
Sie gehören fachlich später in die Verwaltung und werden nicht gelöscht, nur weil sie nicht mehr in der Wachleiter-Fahrzeugakte sichtbar sein sollen.

---

### 6.3 Wachleiter-Fahrzeugmanagement

Der Wachleiter erhält eine operative Fahrzeugakte.

Enthalten:

- Fahrzeugübersicht
- Stammdaten
- Fahrtenbuch
- Tankbelege
- Prüfstatus

Nicht enthalten:

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

Der Fahrzeugstandort wird nicht mehr als eigener Menüpunkt in der Fahrzeugakte geführt.

Die Standortanzeige soll später einsatzbezogen erfolgen, zum Beispiel als kleine Karte in der Einsatzansicht.

---

### 6.4 Fahrzeugübersicht Wachleiter

Die Fahrzeugübersicht wurde fachlich und optisch als operative Einstiegseite ausgerichtet.

Sie soll zeigen:

- Anzahl Fahrzeuge
- aktive Fahrzeuge
- Fahrzeuge in Beobachtung
- nicht verfügbare Fahrzeuge
- Fahrzeugkarte je Fahrzeug
- Status
- Kilometerstand
- direkte Aktionen:
  - Akte
  - Fahrtenbuch
  - Tankbelege
  - Prüfstatus

Die Übersicht ist keine Verwaltungsakte und keine Kostenansicht.

---

### 6.5 Fahrzeugakte Wachleiter

Die Fahrzeugakte enthält:

- operative Kurzansicht
- Fahrzeugstatus
- Kilometerstand
- Prüfstatus-Kurzbewertung
- Tankbelege-Kurzansicht
- Schnellaktionen
- Stammdaten

Die Fahrzeugakte enthält nicht:

- Verträge
- Kostenübersichten
- Versicherung
- Leasing
- Werkstattrechnungen
- allgemeine Dokumentenverwaltung

---

### 6.6 Fahrtenbuch

Das Fahrtenbuch wurde fachlich vereinfacht.

Neue Regel:

Ein Fahrtenbucheintrag wird einmal vollständig erfasst und gespeichert.

Es gibt keinen Zwei-Schritt-Ablauf mehr:

```text
Fahrt anlegen -> später abschließen
```

Stattdessen enthält ein Eintrag:

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

Tankdaten gehören nicht mehr in das Wachleiter-Fahrtenbuch.

Nicht mehr im Wachleiter-Fahrtenbuch:

- Tankmenge in Liter
- Kilometerstand beim Tanken
- Kraftstoffkosten
- spätere Einsatzkosten

Tanken, Liter und spätere Verbrauchsdaten sollen später über Einsatzdokumentation bzw. Tablet-Erfassung entstehen.

Langfristiges Ziel:

`ITW.Einsatz` kann Fahrtenbuchdaten später automatisch oder teilautomatisch vorbereiten.

---

### 6.7 Tankbelege

Im Wachleiterbereich heißt die Funktion fachlich `Tankbelege`.

Technisch kann die vorhandene Fahrzeugdokument-Logik genutzt werden.

Regeln:

- Wachleiter sieht keine allgemeine Dokumentenkategorie-Auswahl.
- Server-seitig wird immer `FahrzeugDokumentKategorie.Tankbeleg` gespeichert.
- Tankbelege brauchen eine Bezeichnung.
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

### 6.8 Prüfstatus

Der Prüfstatus ist kein Terminkalender.

Er ist eine einfache Ampelanzeige für den Wachleiter.

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

Der Prüfstatus soll in der Fahrzeugakte als Kurzbewertung sichtbar sein und zusätzlich auf einer eigenen Seite gepflegt werden können.

---

### 6.9 Fahrzeugcheck und Mängel

Fahrzeugcheck und Mängelerfassung gehören später nicht primär in den Wachleiterbereich.

Fachliche Zielrichtung:

```text
Besatzung im Einsatztablet
-> Fahrzeugcheck vor Dienstbeginn
-> Mängel erfassen
-> Wachleiter sieht offene Mängel
```

Die Besatzung soll später vor jedem Dienst eine Checkliste am Einsatztablet ausfüllen.

Der Wachleiter soll später:

- offene Mängel sehen,
- Einsatzfähigkeit bewerten,
- Fahrzeugstatus setzen,
- Maßnahmen veranlassen.

Der Wachleiter soll aber nicht selbst die komplette Checkliste führen müssen.

---

### 6.10 Verwaltungs-Fahrzeugmanagement

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

Wichtig:

Vorhandene Domain-Entities und Application-Services für Verträge dürfen bestehen bleiben, auch wenn sie im Wachleiterbereich nicht sichtbar sind. Sie können später durch die Verwaltung genutzt werden.

---

### 6.11 Geschäftsführung und spätere Fahrzeugkosten

Später soll nachvollziehbar sein:

- welches Fahrzeug welche Kosten verursacht,
- welcher Einsatz welche Materialkosten verursacht hat,
- welcher Einsatz welche Diesel-/Tankkosten verursacht hat,
- welche weiteren einsatzbezogenen Kosten angefallen sind.

Das gehört nicht in den Wachleiterbereich.

Zuständigkeit:

```text
Verwaltung:
Erfassung und Pflege administrativer Fahrzeugkosten

Geschäftsführung:
spätere Auswertung und Kostenübersicht

Einsatzverwaltung / Materialmanagement:
fachliche Kostenentstehung bei Einsätzen
```

Nur sinnvoll, wenn passende Module freigegeben sind:

- Einsatzverwaltung
- Fahrzeugmanagement
- Materialmanagement

Jetzt wird kein großes Kostenmodul gebaut.

Die vorhandenen Strukturen sollen spätere Verknüpfungen aber nicht blockieren.

---

## 7. Tablet-Tracking

### 7.1 Grundsatzentscheidung

Getrackt wird das mobile Einsatz-Tablet, nicht das Fahrzeug.

Das Fahrzeug ist für GPS-Tracking nicht führend.

Die Tablet-Position entspricht operativ dem Standort des aktuell genutzten ITW.

Eine feste Tablet-Fahrzeug-Zuordnung ist für das Live-Tracking nicht erforderlich.

---

### 7.2 Aktive Tracking-Entities

Aktive Entities:

- `FahrzeugTrackingGeraet`
- `TrackingGeraetStandortAktuell`
- `TrackingGeraetStandortHistorienpunkt`
- `TrackingGeraetEinrichtungscode`

Aktive Tabellen:

- `FahrzeugTrackingGeraete`
- `TrackingGeraetStandorteAktuell`
- `TrackingGeraetStandortHistorie`

Nicht mehr aktiver Tracking-Ansatz:

- `FahrzeugTrackingZuordnung`
- `FahrzeugStandorteAktuell`
- `FahrzeugStandortHistorie`

Alte Tabellen können in der Entwicklungsdatenbank zunächst liegen bleiben, sollen im aktiven Code aber nicht mehr genutzt werden.

---

### 7.3 Aktive Tracking-Services

Aktive Services:

- `RegisterTrackingGeraetService`
- `SaveLocationUpdateService`
- `ReadTabletLiveStandortOverviewService`

Aktiver API-Endpunkt:

```text
POST /api/intensivtransport/fahrzeugmanagement/location-update
```

Erwartete Header:

- `X-Device-Id`
- `X-Api-Key`

Der Endpunkt prüft:

- DeviceIdentifier
- API-Key
- Aktivstatus des Tracking-Geräts

---

### 7.4 Sicherheitsregel

Der API-Key wird nur einmalig nach Registrierung im Klartext angezeigt.

Gespeichert wird nur der Hash.

Wenn der API-Key verloren geht, wird ein neuer API-Key generiert.

---

### 7.5 Tablet-Seite

Aktuelle Tablet-Seite:

```text
/tablet/tracking
```

Sie läuft browserbasiert auf Samsung, Surface oder Tablet.

Sie:

- speichert DeviceIdentifier, API-Key, Intervall und Autostart lokal im Browser,
- fragt Standortfreigabe an,
- sendet automatisch Standortdaten an die ITW-Suite,
- funktioniert nur zuverlässig, solange die Seite aktiv ist.

Aktuell wird keine native App gebaut.

Für den MVP reicht der Browser-Client.

Surface ohne LTE ist möglich, wenn ein mobiler WLAN-Router genutzt wird.

Spätere Optionen:

- native App
- Kiosk-Modus
- QR-Code-/Einrichtungscode-Setup
- vereinfachtes Tablet-Setup für Wachleiter
- Liveansicht als Tracking-Cockpit

---

## 8. Frontend- und CSS-Stand

Die Oberfläche wurde konsolidiert.

Grundregel:

- keine Inline-Styles in Views
- keine Inline-Scripts in Views
- keine neuen Sonder-CSS-Dateien pro View
- Views liefern Struktur und Datenattribute
- Gestaltung liegt zentral in CSS-Dateien
- Verhalten liegt zentral in JavaScript-Dateien

Aktuelle CSS-Struktur:

```text
app-theme.css
app-shell.css
app-components.css
app-modules.css
ITW.Personal.css
ITW.Dienstplan.css
ITW.Fahrzeugmanagement.css
```

Zuständigkeit:

- `app-theme.css` = Tokens / Theme
- `app-shell.css` = Shell / Navigation / Rahmen
- `app-components.css` = wiederverwendbare Standardbausteine
- `app-modules.css` = Shared-Webmodule
- `ITW.Personal.css` = Personal / Mitarbeiter / Urlaubsplaner
- `ITW.Dienstplan.css` = Dienstplan / Wunschkalender / Wachleiterkalender / Auswertungen
- `ITW.Fahrzeugmanagement.css` = Fahrzeugmanagement / Tablet-Tracking / Fahrzeugakte

Wichtig:

Der Split erfolgt bewusst nach echten Fachmodulen, nicht nach einzelnen Views.

---

## 9. Wichtige technische Grenzen

Es gilt weiterhin:

- keine unnötigen neuen Projekte
- keine neuen Layer ohne Bedarf
- bestehende Suite-Struktur beibehalten
- Fachlogik in Fachmodule
- Web-Orchestrierung und ViewModels im Web-Projekt
- EF Core / Datenzugriff in Infrastructure
- `ITW.Infrastructure` darf `ITW.Web` nicht kennen
- gemeinsame Web-Logik nur im Web-Projekt
- keine Logik duplizieren
- keine Schichtvermischung

---

## 10. Nächste sinnvolle Schritte

### 10.1 Technisch kurzfristig

- Build prüfen
- Tests prüfen
- CSS-Split lokal sauber abschließen
- zentrale CSS-Einbindung prüfen
- alte Fahrzeugstandort-Menüpunkte entfernen, falls noch vorhanden
- alte Vertragsrouten aus dem Wachleiterbereich nicht mehr aktiv nutzen
- alte Vertrags-View später entfernen, wenn nicht mehr referenziert

### 10.2 Fachlich danach

- Verwaltungs-Fahrzeugmanagement planen
- Verträge, Policen, Werkstattrechnungen und allgemeine Dokumente in Verwaltung einordnen
- Kostenbasis vorbereiten, aber nicht vorschnell bauen
- Tablet-Fahrzeugcheck später als Besatzungsfunktion planen
- Einsatzbezug später über klare Referenzen herstellen

---

## 11. Zusammenfassung

Der aktuelle Stand ist:

- Dienstplan wurde controllerseitig sauberer getrennt.
- Web-Orchestrierung bleibt im Web-Projekt.
- Modulfreigaben laufen über Bereich + Rolle.
- Wachleiter-Fahrzeugmanagement ist operativ zugeschnitten.
- Verwaltungsinhalte bleiben aus der Wachleiter-Fahrzeugakte raus.
- Fahrtenbuch wird einmal vollständig gespeichert.
- Tankdaten gehören später in Einsatzdoku / Tablet-Erfassung.
- Tablet-Tracking ist tabletbasiert, nicht fahrzeugbasiert.
- CSS wurde von einer großen Moduldatei in echte Moduldateien gesplittet.
- Die nächsten Schritte sind Build/Test-Stabilisierung und danach Planung des Verwaltungs-Fahrzeugmanagements.