# Zielbild der ITW-Suite

## 1. Zweck des Dokuments

Dieses Dokument beschreibt das fachliche und technische Zielbild der ITW-Suite.

Es dient als verbindliche Leitlinie für:

- Architekturentscheidungen
- Zuständigkeiten der Projekte
- Trennung von Bereichen und Modulen
- Rollen- und Sichtbarkeitslogik
- Modulsteuerung
- spätere Erweiterungen

Das Zielbild soll sicherstellen, dass die Suite schrittweise wachsen kann, ohne dass bei jeder Erweiterung die Grundstruktur neu gedacht werden muss.

---

## 2. Grundidee der Suite

Die ITW-Suite ist eine gemeinsame Plattform für mehrere organisatorische Bereiche und fachliche Module.

Die Suite bündelt in einer Anwendung unterschiedliche Themen, unter anderem:

- Intensivtransport
- Verwaltung
- Geschäftsführung

Innerhalb dieser Bereiche existieren fachliche Module, zum Beispiel:

- Dienstplan
- Einsatzverwaltung
- Personal
- Abrechnung
- Buchhaltung
- Lagerlogistik
- Sauerstofflager
- Fahrzeugmanagement
- Tablet-App im Fahrzeug
- später Materialmanagement

Die ITW-Suite ist damit keine lose Sammlung einzelner Tools, sondern eine gemeinsame, fachlich strukturierte Plattform.

---

## 3. Begriffe und Benennung

Für die Suite gilt:

- **Bereiche** sind organisatorische Arbeitsräume.
- **Module** sind fachliche Funktionsblöcke.
- **Rollen** beschreiben die organisatorische Stellung eines Benutzers im jeweiligen Bereich.
- **Modulzuweisungen** legen fest, welche Rolle in welchem Bereich ein Modul nutzen darf.

Wichtig:

- Die geschäftliche Bezeichnung **Geschäftsführung** entspricht im aktuellen technischen Stand dem Bereichscode `Vorstand`.
- Diese technische Benennung ändert nichts an der fachlichen Bedeutung im UI und in den Docs.
- Bereiche und Module sind nicht dasselbe.
- Modulfreigaben laufen direkt über Bereich und Rolle.
- Es gibt keine Mandanten-/Kundenlogik für Modulsichtbarkeit.
- Es gibt keine Modulsichtbarkeit über `MandantId`.

---

## 4. Leitprinzipien

Für die ITW-Suite gelten folgende Grundprinzipien.

### 4.1 Klare fachliche Zuständigkeiten

Jede Logik liegt dort, wo sie fachlich hingehört.

### 4.2 Keine Vermischung von Bereichen

Intensivtransport, Verwaltung und Geschäftsführung bleiben organisatorisch getrennt.

### 4.3 Module statt Funktionschaos

Funktionen werden als fachliche Module gedacht und nicht als lose Einzelansichten.

### 4.4 Einfache Modulsteuerung

Die Sichtbarkeit von Modulen wird zentral und bewusst einfach gesteuert.

### 4.5 Keine Mandanten- oder Kundenlogik

Module werden nicht über Mandanten, Kunden oder Pakete freigeschaltet.

### 4.6 Bestehende Fachlogik erhalten

Neue Modul- und Freigabelogik darf vorhandene fachliche Abläufe nicht kaputtmachen.

Gerade für den Dienstplan ist das entscheidend:

- der Mitarbeiterfluss bleibt eigenständig
- der Wachleiterfluss bleibt eigenständig
- die Modulfreigabe schaltet diese Flüsse nur frei oder aus

### 4.7 Kein unnötiger Architekturausbau

Neue Projekte, Layer, Patterns oder Architekturbausteine werden nur eingeführt, wenn sie fachlich und technisch notwendig sind.

Es gilt:

- bestehende Struktur weiterverwenden
- keine Logik duplizieren
- keine Schichtvermischung
- Fachlogik in Fachmodule
- Web-Orchestrierung und ViewModels im Web-Projekt
- EF Core / Datenzugriff in Infrastructure
- `ITW.Infrastructure` darf `ITW.Web` nicht kennen

---

## 5. Organisatorische Bereiche

Ein Bereich ist ein organisatorisch abgegrenzter Arbeitsraum mit eigener Navigation, eigenen Zuständigkeiten und eigenen Oberflächen.

### 5.1 Intensivtransport

Zuständig für operative und planungsnahe Themen des Intensivtransports.

Typische Inhalte:

- Dienstplan
- Dienstwünsche
- veröffentlichter Plan
- Autoplan
- Personal im Intensivtransport
- Einsatzverwaltung
- operatives Fahrzeugmanagement
- Tablet-App im Fahrzeug
- spätere Lager- und Materialfunktionen
- Sauerstoffdepot

Beim Fahrzeugmanagement bedeutet das:

Der Wachleiter erhält eine operative Fahrzeugakte, aber keine Verwaltungsakte.

### 5.2 Verwaltung

Zuständig für kaufmännische, organisatorische und verwaltungsnahe Themen.

Typische Inhalte:

- Personal in der Verwaltung
- Abrechnung
- Buchhaltung
- spätere kaufmännische Auswertungen
- administrative Fahrzeugverwaltung

Beim Fahrzeugmanagement bedeutet das:

In die Verwaltung gehören später insbesondere:

- Verträge
- Versicherungspolicen
- Leasing
- Finanzierung
- Werkstattrechnungen
- allgemeine Fahrzeugdokumente
- Kosteninformationen
- Abrechnungsdaten

Diese Inhalte gehören nicht in die operative Wachleiter-Fahrzeugakte.

### 5.3 Geschäftsführung

Zuständig für übergeordnete Steuerung und zentrale Freigaben.

Typische Inhalte:

- Führungsebene
- zentrale Modulsteuerung
- zentrale Freigaben
- strategische Steuerung
- Personalübersicht im Bereich Geschäftsführung bei passender Modulfreigabe
- spätere übergreifende Auswertungen

Beim Fahrzeugmanagement bedeutet das:

Die Geschäftsführung erhält später Auswertungen, sofern die passenden Module freigegeben sind.

Beispiele:

- Fahrzeugkosten
- Einsatzkosten
- Materialkosten
- Diesel-/Tankkosten
- übergreifende Kostenentwicklung

Die Geschäftsführung ist dabei nicht die operative Erfassungsstelle.

---

## 6. Fachliche Module

Die Anwendung wird fachlich in Module gegliedert.

### 6.1 Zentrale Plattform

Die Plattform ist zuständig für:

- Anmeldung und Identity
- Benutzerkonto
- Bereichszuordnung
- Bereichsrollen
- Führungsverantwortung
- serverseitige Sichtbarkeit
- Policies / Autorisierung
- Audit
- Benachrichtigungen
- UI-Grundsystem
- zentrale Modulzuweisungen

### 6.2 Modul Dienstplan

Zuständig für:

- Planungsperioden
- offene und geschlossene Wunschphasen
- Dienstwünsche
- gewünschte Dienstanzahl
- Planungsregeln
- automatische und manuelle Planung
- Freigabe und Veröffentlichung

Wichtige Festlegung:

- Dienstwünsche gehören fachlich zum Modul **Dienstplan**.
- Dienstwünsche bilden kein eigenes Modul.
- Dienstwünsche bilden keinen eigenen Bereich.
- Mitarbeiter- und Wachleiterfluss bleiben getrennt.

### 6.3 Modul Personal

Zuständig für:

- Mitarbeiterprofile
- Qualifikationen
- bereichsbezogene Personaldaten
- spätere personalbezogene Zusatzfunktionen

Wichtig:

- Personal kann unabhängig vom Dienstplan freigegeben werden.
- Im Intensivtransport kann Personal z. B. nur für Wachleiter sichtbar sein.
- Dienstplanfreigabe bedeutet nicht automatisch Personalzugriff.

### 6.4 Modul Einsatzverwaltung

Zuständig für:

- Einsätze
- Besetzungen
- Status
- Einsatzdaten
- spätere Einsatzdokumentation

Die Einsatzverwaltung kann später die Live-Position des Tablets anzeigen, besitzt aber nicht automatisch die fachliche Verantwortung für das Tracking.

Langfristig kann die Einsatzverwaltung später Daten liefern oder übernehmen für:

- einsatzbezogene Fahrtenbuchdaten
- einsatzbezogene Tank-/Verbrauchsdaten
- einsatzbezogene Materialverbräuche
- spätere Einsatzkosten

Diese Themen werden aber nicht vorschnell in das Wachleiter-Fahrzeugmanagement eingebaut.

### 6.5 Modul Fahrzeugmanagement

Das Modul **Fahrzeugmanagement** ist zuständig für fahrzeug- und tabletbezogene Funktionen.

Wichtig:

Das Fahrzeugmanagement ist nicht nur ein Wachleiter-Modul.  
Es hat je nach Bereich unterschiedliche fachliche Ausprägungen.

Es wird unterschieden zwischen:

```text
Intensivtransport / Wachleiter:
operative Fahrzeugakte

Verwaltung:
administrative Fahrzeugverwaltung

Geschäftsführung:
spätere Auswertungen und Kostenübersich

6.5.1 Intensivtransport / Wachleiter

Der Wachleiter erhält eine operative Fahrzeugakte.

Enthalten:

Fahrzeugübersicht
Stammdaten
Fahrtenbuch
Tankbelege
Prüfstatus

Nicht enthalten:

Verträge
Versicherungspolicen
Leasing
Finanzierung
Werkstattrechnungen
allgemeine Fahrzeugdokumente
Kostenberichte
Buchhaltung
MPG-Verwaltung
Verwaltungskalender
separate Fahrzeugstandort-Seite

Der Fahrzeugstandort wird nicht mehr als eigener Menüpunkt in der Fahrzeugakte geführt.

Später soll der Standort einsatzbezogen angezeigt werden, z. B. als kleine Karte in der Einsatzansicht.

6.5.2 Fahrtenbuch im Wachleiterbereich

Das Fahrtenbuch ist im Wachleiterbereich bewusst einfach.

Ein Fahrtenbucheintrag wird einmal vollständig gespeichert.

Es gibt keinen Zwei-Schritt-Ablauf mehr:

Fahrt anlegen -> später abschließen

Ein Eintrag enthält:

Kategorie
Fahrtzweck
Startzeit
Endzeit
Fahrer
Beifahrer
Von
Nach
Startkilometerstand
Endkilometerstand
Bemerkung

Nicht im Wachleiter-Fahrtenbuch enthalten:

Tankmenge in Liter
Kilometerstand beim Tanken
Kraftstoffkosten
spätere Einsatzkosten

Tankdaten sollen später über Einsatzdokumentation bzw. Tablet-Erfassung entstehen.

6.5.3 Tankbelege im Wachleiterbereich

Im Wachleiterbereich heißt die Funktion fachlich Tankbelege.

Technisch kann die vorhandene Fahrzeugdokument-Logik genutzt werden.

Regeln:

Wachleiter sieht keine allgemeine Dokumentenkategorie-Auswahl.
Server-seitig wird immer FahrzeugDokumentKategorie.Tankbeleg gespeichert.
Tankbelege brauchen eine Bezeichnung.
Tankbelege werden nach Upload-Datum sortiert, neueste zuerst.
Dateien liegen im Server-Dateisystem.
Die Datenbank speichert nur Metadaten und Speicherpfad.
Keine Datei-Inhalte / Blobs in der Datenbank.

Ablage:

App_Data/Fahrzeugdokumente/{FahrzeugId}/{Guid}.{endung}

Allgemeine Fahrzeugdokumente gehören später in die Verwaltung.

6.5.4 Prüfstatus im Wachleiterbereich

Der Prüfstatus ist kein Terminkalender.

Er ist eine einfache Ampelanzeige für den Wachleiter.

Prüfpunkte:

HU/AU
Sicherheitsprüfung elektrische Anlage
Sicherheitsprüfung Sauerstoffanlage
Sicherheitsprüfung Aufbau
Service allgemein

Nicht enthalten:

MPG
Verträge
Rechnungen
Werkstattkalender
Betreiberkalender
Uhrzeiten

Ampellogik:

Grün = mehr als 30 Tage Zeit
Gelb = innerhalb von 30 Tagen fällig
Rot = überfällig
Grau = nicht hinterlegt
6.5.5 Verwaltung / administrative Fahrzeugverwaltung

Die Verwaltung erhält später die administrative Fahrzeugverwaltung.

Dort gehören hin:

Verträge
Versicherungspolicen
Leasing
Finanzierung
Werkstattrechnungen
allgemeine Fahrzeugdokumente
Kosteninformationen
Abrechnungsdaten

Wichtig:

Vorhandene Domain-Entities und Application-Services für Verträge dürfen bestehen bleiben.
Sie werden nicht gelöscht, nur weil Verträge im Wachleiterbereich nicht mehr sichtbar sind.

6.5.6 Geschäftsführung / spätere Auswertung

Die Geschäftsführung erhält später Auswertungen, sofern die passenden Module freigegeben sind.

Später soll nachvollziehbar sein:

welches Fahrzeug welche Kosten verursacht,
welcher Einsatz welche Materialkosten verursacht hat,
welcher Einsatz welche Diesel-/Tankkosten verursacht hat,
welche weiteren einsatzbezogenen Kosten angefallen sind.

Dafür sind später insbesondere relevant:

Einsatzverwaltung
Fahrzeugmanagement
Materialmanagement

Jetzt wird kein großes Kostenmodul gebaut.
Die Strukturen sollen spätere Verknüpfungen aber nicht blockieren.

6.5.7 Tablet-Tracking

Wichtige fachliche Festlegung:

Das GPS-Tracking gehört fachlich zum mobilen Einsatz-Tablet.
Es wird nicht primär ein Fahrzeug getrackt.
Der Wachleiter weiß organisatorisch, welches Fahrzeug aktuell eingesetzt ist.
Die Tablet-Position entspricht im operativen Alltag der Position des aktuell genutzten ITW.
Eine feste Tablet-Fahrzeug-Zuordnung ist für das Live-Tracking nicht erforderlich.

Damit gilt:

Das Tracking-Gerät wird über DeviceIdentifier und API-Key registriert.
Das Tablet sendet Standortdaten an die ITW-Suite.
Die ITW-Suite speichert aktuellen Tablet-Standort und Streckenhistorie.
Der Wachleiter sieht Standort, Strecke und Bewegungsstatus in der Liveansicht.

Aktive Tracking-Entities:

FahrzeugTrackingGeraet
TrackingGeraetStandortAktuell
TrackingGeraetStandortHistorienpunkt
TrackingGeraetEinrichtungscode

Nicht mehr aktiver Tracking-Ansatz:

FahrzeugTrackingZuordnung
FahrzeugStandorteAktuell
FahrzeugStandortHistorie

Alte Tabellen können in der Entwicklungsdatenbank zunächst liegen bleiben, sollen im aktiven Code aber nicht mehr genutzt werden.

6.6 Modul Tablet-App im Fahrzeug

Das Modul Tablet-App im Fahrzeug beschreibt die spätere Nutzung des mobilen Tablets im Fahrzeug.

Aktuell wird dafür keine zusätzliche native Android-App gebaut.
Der erste Zielweg ist eine browserbasierte Tablet-Seite innerhalb der ITW-Suite.

Diese Tablet-Seite:

läuft im Browser des Tablets,
speichert Device-Identifier und API-Key lokal im Browser,
fragt die Standortfreigabe an,
sendet den Standort automatisch an die ITW-Suite,
kann später auf einem neuen Tablet erneut eingerichtet werden.

Wichtig:

Der Browser-Client ist für den aktuellen Ausbau ausreichend.
Dauerhaftes Tracking bei gesperrtem Gerät ist browserseitig nur eingeschränkt zuverlässig.
Eine native App oder Kiosk-/Gerätekonfiguration bleibt eine spätere Option, ist aber aktuell kein Bestandteil des MVP.
Es wird keine zusätzliche App eingeführt, solange die browserbasierte Lösung fachlich ausreicht.

Später soll das Einsatztablet außerdem für operative Besatzungsfunktionen genutzt werden.

Dazu gehört insbesondere:

Besatzung im Einsatztablet
-> Fahrzeugcheck vor Dienstbeginn
-> Mängel erfassen
-> Wachleiter sieht offene Mängel

Der Fahrzeugcheck und die Mängelerfassung gehören fachlich in die Hände der Besatzung, nicht primär in den Wachleiterbereich.

Der Wachleiter soll später:

offene Mängel sehen,
Einsatzfähigkeit bewerten,
Fahrzeugstatus setzen,
Maßnahmen veranlassen.
6.7 Weitere geplante Module

Langfristig sind weitere Module vorgesehen, zum Beispiel:

Abrechnung
Buchhaltung
Lagerlogistik
Sauerstofflager
Materialmanagement

Diese Module werden zentral gepflegt und direkt über Bereich und Rolle freigeschaltet.

7. Trennung von Bereichen und Modulen

Für die ITW-Suite gilt ausdrücklich:

Bereiche sind organisatorische Sicht- und Zuständigkeitsräume.
Module sind fachliche Einheiten mit eigener Daten- und Logikverantwortung.
Modulzuweisungen legen fest, welche Rolle in welchem Bereich ein Modul nutzen darf.

Beispiele:

Intensivtransport ist ein Bereich.
Dienstplan ist ein Modul innerhalb dieses Bereichs.
Dienstwünsche sind ein fachlicher Teil des Moduls Dienstplan.
Personal ist ein eigenes Modul.
Fahrzeugmanagement ist ein eigenes Modul.
Abrechnung ist ein Modul der Verwaltung.

Damit gilt:

Bereich und Rolle beschreiben, wer ein Benutzer organisatorisch ist.
Modulzuweisungen beschreiben, welche Funktionen dieser Benutzer zusätzlich nutzen darf.
8. Rollen- und Modulfreigabelogik

Die Suite unterscheidet zwei Ebenen.

8.1 Organisatorische Zuordnung

Ein Benutzer hat:

einen Bereich
eine Bereichsrolle
optional Führungsverantwortung
8.2 Fachliche Modulfreigabe

Zusätzlich wird zentral gesteuert, welche Module für diese Bereichsrolle sichtbar und nutzbar sind.

Beispiele:

Dienstplan für Intensivtransport / Mitarbeiter
Dienstplan für Intensivtransport / Wachleiter
Personal für Intensivtransport / Wachleiter
Fahrzeugmanagement für Intensivtransport / Wachleiter
Fahrzeugmanagement für Verwaltung / Verwaltungsmitarbeiter
Fahrzeugmanagement für Verwaltung / Bereichsleitung
Personal für Verwaltung / Bereichsleitung
Abrechnung für Verwaltung / Verwaltungsmitarbeiter

Wichtig:

Module werden nicht über Mandanten gesteuert.
Module werden nicht über Kundenlogik gesteuert.
Module werden nicht nur über Menüs versteckt.
Module werden serverseitig anhand von Bereich, Rolle und Modulzuweisung geprüft.
Der aktuelle Benutzerkontext kennt die aktiven Module.
9. Dienstplan bleibt fachlich getrennt

Für den Dienstplan gilt weiterhin ausdrücklich:

9.1 Mitarbeiterfluss

Bleibt:

Dienstwünsche
veröffentlichter Plan
9.2 Wachleiterfluss

Bleibt:

Dienstplan
veröffentlichter Plan
Autoplan

Die Modulfreigabe Dienstplan darf diese fachliche Trennung nicht aufheben.

Das bedeutet:

Ein Mitarbeiter erhält durch Dienstplan nicht automatisch Wachleiterfunktionen.
Ein Wachleiter erhält durch Dienstplan nicht automatisch Personalfunktionen.
Personal bleibt ein eigenständiges Modul.
Fahrzeugmanagement bleibt ebenfalls ein eigenständiges Modul.
10. Gemeinsame Identity-Basis

Es gibt nur eine zentrale Identity-Datenbank.

Die zentrale Identity ist zuständig für:

Anmeldung
Benutzerkonto
Passwort- und Sicherheitslogik
Kontoaktivierung / Sperrung
technische Identitätsdaten

Die zentrale Identity ist nicht der Ablageort für alle fachlichen Zusatzdaten.

Wichtig:

Modulfreigaben werden nicht an der Identity aufgehängt.
Modulfreigaben werden nicht über Kunden- oder Mandantenlogik gesteuert.
Modulfreigaben werden als eigene fachliche Zuweisungen über Bereich und Rolle verwaltet.
11. Bereichsbezogene Zusatzdaten

Neben der zentralen Identity werden fachliche Zusatzdaten getrennt geführt.

Dazu gehören zum Beispiel:

Bereichszugehörigkeit
Bereichsrolle
Führungsverantwortung
Qualifikationen
modulbezogene Fachdaten
Modulzuweisungen für Bereich und Rolle

Diese Daten werden nicht pauschal in der Identity gespeichert, sondern in fachlich zuständigen Modellen bzw. Modulen.

12. UI-Zielbild

Für die Oberfläche gilt:

jede Area erhält ein eigenes Layout innerhalb des gemeinsamen UI-Systems
Navigation und Dashboards bleiben bereichsbezogen
Module werden zusätzlich zur Bereichslogik sichtbar oder unsichtbar geschaltet
serverseitiger Modulschutz ergänzt die Navigation
die Oberfläche darf nie allein für Sicherheit verantwortlich sein

Wichtig:

gemeinsame Web-Logik bleibt im Web-Projekt
Styles und Scripts werden zentral gehalten
fachliche Trennung bleibt trotz gemeinsamer Shell erhalten
Views enthalten grundsätzlich Struktur und Daten, aber keine eigenen Inline-Styles oder Inline-Scripts
zentrale Ressourcen werden über die gemeinsamen Resource-Partials eingebunden
modulbezogenes Verhalten liegt in zentralen JS-Dateien
modulbezogene Gestaltung liegt in zentralen CSS-Dateien

Aktuelle CSS-Struktur:

app-theme.css
app-shell.css
app-components.css
app-modules.css
ITW.Personal.css
ITW.Dienstplan.css
ITW.Fahrzeugmanagement.css

Zuständigkeit:

app-theme.css = Tokens / Theme
app-shell.css = Shell / Navigation / Rahmen
app-components.css = wiederverwendbare Standardbausteine
app-modules.css = Shared-Webmodule
ITW.Personal.css = Personal / Mitarbeiter / Urlaubsplaner
ITW.Dienstplan.css = Dienstplan / Wunschkalender / Wachleiterkalender / Auswertungen
ITW.Fahrzeugmanagement.css = Fahrzeugmanagement / Tablet-Tracking / Fahrzeugakte

Wichtig:

Der Split erfolgt bewusst nach echten Fachmodulen, nicht nach einzelnen Views.

13. Zielzustand der Modulsteuerung

Die Geschäftsführung steuert zentral, welche Rolle in welchem Bereich ein Modul nutzen darf.

Das bedeutet:

keine Mandantenpakete
keine Kundenpakete
keine abgeleitete Sichtbarkeit über MandantId
keine Vermischung von organisatorischer Zuordnung und fachlicher Modulfreigabe

Die Modulsteuerung ist damit bewusst einfach, fachnah und nachvollziehbar aufgebaut.

14. Zusammenfassung

Das Zielbild der ITW-Suite ist:

eine gemeinsame Suite mit klar getrennten Bereichen
fachlich sauber geschnittene Module
eine einfache zentrale Modulsteuerung
stabile und verständliche Rollen- und Sichtbarkeitsregeln
keine Mandanten- oder Kundenlogik für die Modulsichtbarkeit
Erhalt der bestehenden Fachflüsse, insbesondere im Dienstplan
klar getrenntes Fahrzeugmanagement für Wachleiter, Verwaltung und spätere Geschäftsführungsauswertung
operative Fahrzeugakte für den Wachleiter
administrative Fahrzeugverwaltung für die Verwaltung
spätere Kosten- und Einsatzkosten-Auswertung für die Geschäftsführung
tabletbasiertes GPS-Tracking im Fahrzeugmanagement
browserbasierte Tablet-Seite ohne zusätzliche native App im MVP
zentrale UI-Struktur mit echten Modul-CSS-Dateien

Die Geschäftsführung steuert zentral, welche Rolle in welchem Bereich ein Modul bekommt.
Die operative Nutzung bleibt in den zuständigen Bereichen und Rollen.