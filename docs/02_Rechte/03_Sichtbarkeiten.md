# Sichtbarkeiten und Modulzugriffe

## 1. Zweck dieses Dokuments

Dieses Dokument beschreibt, welche Benutzer in welchem Bereich welche Oberflächen und Module sehen dürfen.

Es ergänzt:

- Rollenmodell
- Zuständigkeitsmatrix
- Modulfreigaben
- Bereichsnavigation
- serverseitige Autorisierung

Wichtig: Sichtbarkeit im Menü ersetzt niemals serverseitigen Schutz.

---

## 2. Grundsatz

Für alle Bereiche gilt:

- Ein Benutzer sieht nur Inhalte seines aktuellen Bereichs.
- Ein Benutzer sieht nur Funktionen seiner aktuellen Bereichsrolle.
- Ein Benutzer sieht nur Module, die für Bereich und Rolle aktiv freigegeben sind.
- Controller-Zugriffe müssen serverseitig geschützt sein.
- Navigation darf nur erlaubte Funktionen anzeigen, ist aber nicht die Sicherheitsgrenze.

---

## 3. Bereiche

Aktuelle organisatorische Bereiche:

- Intensivtransport
- Verwaltung
- Geschäftsführung

Die technische Benennung der Geschäftsführung kann intern weiterhin `Vorstand` lauten.  
Im UI und in der Dokumentation wird fachlich von Geschäftsführung gesprochen.

---

## 4. Grundregel für Module

Module werden direkt über Bereich und Rolle freigeschaltet.

Beispiele:

- `Dienstplan` für `Intensivtransport / Mitarbeiter` zeigt Mitarbeiter-Dienstplanfunktionen.
- `Dienstplan` für `Intensivtransport / Wachleiter` zeigt Wachleiter-Dienstplanfunktionen.
- `Personal` für `Intensivtransport / Wachleiter` zeigt Personal-/Mitarbeiterfunktionen des Intensivtransports.
- `Fahrzeugmanagement` für `Intensivtransport / Wachleiter` zeigt die operative Fahrzeugakte des Wachleiters.
- `Fahrzeugmanagement` für `Verwaltung / Verwaltungsmitarbeiter` zeigt später administrative Fahrzeugverwaltungsfunktionen.
- `Fahrzeugmanagement` für `Verwaltung / Bereichsleitung` zeigt später administrative Fahrzeugverwaltungs- und Kontrollfunktionen.
- `Abrechnung` für `Verwaltung / Verwaltungsmitarbeiter` zeigt Abrechnungsfunktionen.
- `Personal` für `Verwaltung / Bereichsleitung` zeigt Personalverwaltungsfunktionen der Verwaltung.

Nicht erlaubt:

- Modulfreigabe über MandantId
- Modulfreigabe über Kundenlogik
- Modulfreigabe nur über Menüverstecken
- automatische Freigabe aller Module wegen Führungsrolle

---

## 5. Geschäftsführung

Die Geschäftsführung ist für zentrale Steuerung zuständig.

Darf sehen:

- Geschäftsführungsdashboard
- zentrale Benutzer-/Organisationssichten, soweit freigegeben
- zentrale Modulsteuerung
- zentrale Freigaben
- übergeordnete Auswertungen, sobald vorhanden

Darf nicht automatisch sehen:

- operative Dienstplanfunktionen ohne passende Freigabe
- operative Wachleiterfunktionen ohne passende Freigabe
- Bereichsdaten ohne fachliche Zuständigkeit
- technische Volladmin-Funktionen nur aufgrund der Geschäftsführungsrolle

Wichtig:

- Geschäftsführung steuert Module.
- Geschäftsführung ist nicht automatisch technischer Volladmin.
- Modulfreigaben bleiben explizit.

Später möglich bei passenden Modulfreigaben:

- Fahrzeugkosten-Auswertung
- Einsatzkosten-Auswertung
- Materialkosten-Auswertung
- übergreifende wirtschaftliche Auswertungen

Diese Auswertungen setzen fachlich passende Module voraus, z. B.:

- `Einsatzverwaltung`
- `Fahrzeugmanagement`
- `Materialmanagement`

---

## 6. Intensivtransport / Mitarbeiter

Mitarbeiter im Intensivtransport sehen nur ihren eigenen fachlichen Arbeitsbereich.

Bei freigegebenem Modul `Dienstplan` dürfen sie sehen:

- Dienstwünsche
- veröffentlichter Plan
- eigene dienstplanbezogene Informationen

Sie dürfen nicht sehen:

- Wachleiterkalender
- Autoplan
- manuelle Planungsfunktionen
- Dienstplanfreigaben
- Personalverwaltung
- Fahrzeugmanagement-Wachleiterfunktionen
- Tracking-Tablet-Verwaltung
- Tablet-Live-Standort
- Verwaltungsmodule
- Geschäftsführungsfunktionen

Später kann das Einsatztablet bzw. die Tablet-App im Fahrzeug eigene Besatzungsfunktionen erhalten.

Dazu gehören perspektivisch:

- Fahrzeugcheck vor Dienstbeginn
- Mängelerfassung
- einsatzbezogene Dokumentation
- spätere Tank-/Verbrauchserfassung

Diese Funktionen sind nicht automatisch Bestandteil der normalen Mitarbeiter-Webansicht.

---

## 7. Intensivtransport / Wachleiter

Der Wachleiter besitzt operative Führungsfunktionen im Bereich Intensivtransport.

### 7.1 Dienstplan

Bei freigegebenem Modul `Dienstplan` darf der Wachleiter sehen:

- Dienstplan
- Wachleiterkalender
- veröffentlichter Plan
- Autoplan
- Planungsmodal
- Tagesplanung
- Ausfallverarbeitung
- Dienstplan-Auswertungen, soweit vorgesehen

Der Wachleiter darf dadurch nicht automatisch sehen:

- Personalmodul
- Fahrzeugmanagement
- Verwaltung
- Geschäftsführung

---

### 7.2 Personal

Bei freigegebenem Modul `Personal` darf der Wachleiter sehen:

- ITW-Mitarbeiterübersicht
- Mitarbeiterdetailseite
- ITW-bezogene Qualifikationen
- bereichsbezogene Personaldaten des Intensivtransports

Der Wachleiter darf dadurch nicht automatisch sehen:

- Dienstplan, wenn `Dienstplan` nicht freigegeben ist
- Verwaltungspersonal
- Geschäftsführungsfunktionen

---

### 7.3 Fahrzeugmanagement

Bei freigegebenem Modul `Fahrzeugmanagement` darf der Wachleiter das operative Fahrzeugmanagement des Intensivtransports sehen.

Dazu gehören:

- Fahrzeugmanagement-Menü
- Fahrzeugübersicht
- operative Fahrzeugakte
- Stammdaten
- Fahrtenbuch
- Tankbelege
- Prüfstatus
- Tracking-Tablets
- Registrierung neuer Tracking-Tablets
- einmalige Anzeige neu erzeugter API-Keys
- letzter Kontakt eines Tablets
- Online-/Offline-Status
- Bewegungsstatus `Fährt` oder `Steht`
- aktuelle Tablet-Position
- Streckenverlauf der laufenden Route

Der Wachleiter darf im Fahrzeugmanagement nicht sehen:

- Fahrzeugmanagement ohne Modulfreigabe
- Tracking-Tablet-Verwaltung anderer Bereiche
- technische API-Keys nach der einmaligen Anzeige erneut im Klartext
- interne API-Key-Hashes
- Datenbank- oder Infrastrukturdetails
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

Wichtig:

Der Wachleiter führt die operative Fahrzeugakte.

Er führt nicht die administrative Fahrzeugverwaltung.

---

### 7.4 Fahrtenbuch im Wachleiterbereich

Das Fahrtenbuch im Wachleiterbereich ist bewusst einfach.

Ein Fahrtenbucheintrag wird einmal vollständig gespeichert.

Es gibt keinen Zwei-Schritt-Ablauf mehr:

```text
Fahrt anlegen -> später abschließen
```

Der Wachleiter darf erfassen:

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

Der Wachleiter erfasst im Fahrtenbuch nicht:

- Tankmenge in Liter
- Kilometerstand beim Tanken
- Kraftstoffkosten
- Einsatzkosten

Tankdaten und Verbrauchsdaten sollen später über Einsatzdokumentation bzw. Tablet-Erfassung entstehen.

---

### 7.5 Tankbelege im Wachleiterbereich

Bei freigegebenem Modul `Fahrzeugmanagement` darf der Wachleiter Tankbelege verwalten.

Dazu gehört:

- Tankbeleg hochladen
- Bezeichnung erfassen
- Tankbeleg anzeigen
- Tankbeleg herunterladen
- Tankbeleg löschen, soweit erlaubt

Der Wachleiter darf dabei keine allgemeine Dokumentenkategorie auswählen.

Serverseitig wird im Wachleiterbereich immer gespeichert:

```text
FahrzeugDokumentKategorie.Tankbeleg
```

Allgemeine Fahrzeugdokumente gehören später in die Verwaltung.

---

### 7.6 Prüfstatus im Wachleiterbereich

Der Wachleiter darf im Fahrzeugmanagement den einfachen Prüfstatus sehen und pflegen.

Prüfpunkte:

- HU/AU
- Sicherheitsprüfung elektrische Anlage
- Sicherheitsprüfung Sauerstoffanlage
- Sicherheitsprüfung Aufbau
- Service allgemein

Der Wachleiter sieht hier nicht:

- MPG
- Verträge
- Rechnungen
- Werkstattkalender
- Betreiberkalender
- Uhrzeiten

Der Prüfstatus ist kein Terminkalender, sondern eine einfache Ampelanzeige.

Ampellogik:

- Grün = mehr als 30 Tage Zeit
- Gelb = innerhalb von 30 Tagen fällig
- Rot = überfällig
- Grau = nicht hinterlegt

---

### 7.7 Fahrzeugcheck und Mängel

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

Die Checkliste selbst gehört nicht primär in die Wachleiter-Weboberfläche.

---

## 8. Verwaltung

Die Verwaltung ist zuständig für verwaltungsnahe und kaufmännische Funktionen.

Bei passenden Modulfreigaben darf sie sehen:

- Verwaltungsdashboard
- verwaltungsbezogene Personalansichten
- Abrechnung
- Buchhaltung
- spätere kaufmännische Auswertungen
- administrative Fahrzeugverwaltung, sobald umgesetzt

Die Verwaltung darf nicht automatisch sehen:

- Intensivtransport-Dienstplan
- Wachleiterfunktionen
- Tablet-Live-Standort
- operative Fahrzeugmanagementfunktionen des Intensivtransports
- Geschäftsführungsfreigaben ohne passende Rolle

---

### 8.1 Fahrzeugmanagement in der Verwaltung

Bei freigegebenem Modul `Fahrzeugmanagement` darf die Verwaltung später administrative Fahrzeugverwaltungsfunktionen sehen.

Dazu gehören perspektivisch:

- Verträge
- Versicherungspolicen
- Leasing
- Finanzierung
- Werkstattrechnungen
- allgemeine Fahrzeugdokumente
- Kosteninformationen
- Abrechnungsdaten

Diese Funktionen gehören nicht in den Wachleiterbereich.

Wichtig:

Vorhandene Domain-Entities und Application-Services für Fahrzeugverträge dürfen bestehen bleiben.  
Sie werden später durch die Verwaltung genutzt und nicht in den Wachleiterbereich zurückgebaut.

---

## 9. Bereichsleitung Verwaltung

Die Bereichsleitung Verwaltung kann je nach Modulfreigabe erweiterte Verwaltungsfunktionen sehen.

Dazu können gehören:

- Benutzer-/Organisationssichten der Verwaltung
- Verwaltungsmitarbeiter
- kaufmännische Module
- administrative Fahrzeugverwaltung
- spätere Auswertungen

Nicht automatisch enthalten:

- Intensivtransport-Wachleiterfunktionen
- operative Dienstplanung
- Tablet-Tracking
- Geschäftsführungsfunktionen

---

## 10. Tablet-Tracking-Seite

Die Tablet-Tracking-Seite ist eine technische Geräteseite.

Aktueller Pfad:

```text
/tablet/tracking
```

Sie dient dazu:

- Standortfreigabe im Browser des Tablets einzuholen,
- Device-Identifier und API-Key lokal im Browser zu speichern,
- Standortdaten automatisch an die ITW-Suite zu senden.

Die Tablet-Tracking-Seite ist keine Wachleiter-Oberfläche.

Wichtig:

- Die Seite darf Standortdaten nur mit gültigem Device-Identifier und gültigem API-Key schreiben.
- Der API-Endpunkt prüft serverseitig die Registrierung.
- Der API-Endpunkt prüft den API-Key.
- Der API-Endpunkt prüft den Aktivstatus des Tracking-Geräts.
- Ein Gerät ohne gültige Registrierung darf keine Standortdaten schreiben.

Aktuelle fachliche Entscheidung:

- Getrackt wird das mobile Einsatz-Tablet.
- Es wird nicht primär das Fahrzeug getrackt.
- Eine feste Tablet-Fahrzeug-Zuordnung ist für das Live-Tracking nicht erforderlich.
- Die Tablet-Position entspricht operativ dem Standort des aktuell genutzten ITW.

---

## 11. API-Zugriff für Standortdaten

Der Standort-Endpunkt ist technisch erreichbar, aber fachlich abgesichert.

Aktiver Endpunkt:

```text
POST /api/intensivtransport/fahrzeugmanagement/location-update
```

Erwartete Header:

```text
X-Device-Id
X-Api-Key
```

Der Endpunkt darf Standortdaten nur speichern, wenn:

- das Tracking-Gerät registriert ist,
- das Tracking-Gerät aktiv ist,
- der API-Key gültig ist.

Der API-Zugriff ist damit nicht über Benutzerlogin, sondern über Gerätekennung und API-Key abgesichert.

Sicherheitsregel:

- Der API-Key wird nur einmalig nach Registrierung im Klartext angezeigt.
- Gespeichert wird nur der Hash.
- Wenn der API-Key verloren geht, wird ein neuer API-Key generiert.

---

## 12. Navigation

Die Bereichsnavigation wird anhand folgender Informationen aufgebaut:

- aktueller Bereich
- aktuelle Rolle
- aktive Module des aktuellen Benutzerkontexts
- aktueller Controller
- aktuelle Action

Wichtig:

- Module erscheinen nur, wenn sie für Bereich und Rolle freigegeben sind.
- Navigation allein reicht nicht als Schutz.
- Controller müssen serverseitig prüfen.
- `RequireModule` schützt modulbezogene Controller zusätzlich.

Für das Fahrzeugmanagement des Wachleiters gilt:

- `Fahrzeuge` bleibt sichtbar.
- `Tablets` bleibt sichtbar, wenn Tracking-Geräte verwaltet werden dürfen.
- Eine separate Fahrzeugstandort-Seite soll nicht mehr als eigener Menüpunkt im Fahrzeugmanagement geführt werden.
- Standortinformationen sollen später einsatzbezogen angezeigt werden.

---

## 13. CurrentUserContext

Der aktuelle Benutzerkontext enthält neben Benutzer, Bereich und Rolle auch aktive Module.

Zugriffsbezogene Tests müssen deshalb setzen:

- aktueller Benutzer
- aktueller Bereich
- aktuelle Rolle
- aktive Module

Typische Fehlerursachen bei Tests:

- Bereich fehlt
- Rolle fehlt
- Modul fehlt
- Controller-Ziel wurde nach Refactor geändert
- alte Weiterleitung wird noch erwartet
- neue Modulfreigabe wird im Test nicht gesetzt
- Navigation wird getestet, aber serverseitiger Modulschutz nicht berücksichtigt

---

## 14. Zusammenfassung

Sichtbarkeit in der ITW-Suite folgt diesen Regeln:

- Bereich bestimmt den organisatorischen Arbeitsraum.
- Rolle bestimmt die fachliche Stellung.
- Modulfreigabe bestimmt die zusätzlich sichtbaren Funktionen.
- Controller schützen serverseitig.
- Menüs bilden nur erlaubte Funktionen ab.
- Module werden direkt über Bereich und Rolle freigegeben.
- Es gibt keine Mandanten-/Kundenlogik für Modulsichtbarkeit.
- Tablet-Tracking gehört zum Fahrzeugmanagement.
- Getrackt wird das mobile Einsatz-Tablet, nicht primär das Fahrzeug.
- Wachleiter sehen operative Fahrzeugmanagementfunktionen nur mit Modulfreigabe `Fahrzeugmanagement`.
- Mitarbeiter sehen keine Wachleiter-Trackingfunktionen.
- Verwaltung erhält später administrative Fahrzeugverwaltungsfunktionen.
- Geschäftsführung erhält später Auswertungen, aber keine automatische operative Vollsicht.