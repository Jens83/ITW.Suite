<!-- Datei: docs/03_MVP/01_Bauphasen.md -->
# Bauphasen für den Neuaufbau der ITW-Suite

## Zweck dieses Dokuments

Dieses Dokument beschreibt die empfohlene Umsetzungsreihenfolge für den Neuaufbau der ITW-Suite.

Ziel ist, die Lösung kontrolliert und strukturiert aufzubauen, ohne frühzeitig technische oder fachliche Grenzen zu verwischen.

---

## 1. Grundregel

Die ITW-Suite wird nicht featureweise ungeordnet aufgebaut, sondern in klaren Bauphasen.

Jede Phase schafft Voraussetzungen für die nächste Phase.

---

## 2. Phase 0 – Architektur und Struktur festziehen

## Ziel
Verbindliche Grundlage für alle weiteren Umsetzungen schaffen.

## Inhalte
- Zielbild dokumentieren
- Projektgrenzen dokumentieren
- Referenzmatrix dokumentieren
- Datenmodell konzeptionell festlegen
- Rollenmodell festlegen
- Sichtbarkeitsregeln festlegen
- MVP-Schnitt definieren

## Ergebnis
- Architektur ist verbindlich dokumentiert
- Projektstruktur ist angelegt
- Referenzrichtungen sind festgelegt
- spätere Strukturbrüche werden vermieden

---

## 3. Phase 1 – Solution und technische Grundstruktur

## Ziel
Die technische Grundstruktur der neuen Lösung sauber aufsetzen.

## Inhalte
- Solution anlegen
- Projekte anlegen
- Projektverweise setzen
- Ordnerstruktur pro Projekt anlegen
- Areas im Web-Projekt anlegen
- Layout- und Navigationsgrundstruktur vorbereiten

## Ergebnis
- technische Basis steht
- saubere Projektstruktur existiert
- Web-Areas sind vorbereitet

---

## 4. Phase 2 – Plattformfundament

## Ziel
Die zentrale Plattform der Anwendung aufbauen.

## Inhalte
- zentrale Identity anbinden
- Benutzerkonto-Grundlage schaffen
- Bereichszuordnung modellieren
- Bereichsrollen modellieren
- globale Rollen festlegen
- Policies und Scopes vorbereiten
- Audit-Grundlage schaffen
- Benachrichtigungsgrundlage schaffen

## Ergebnis
- zentrale Benutzer- und Organisationsbasis ist vorhanden
- Sichtbarkeit kann serverseitig geprüft werden
- Plattformthemen sind nicht mehr unstrukturiert verteilt

---

## 5. Phase 3 – Web-Grundsystem

## Ziel
Die Anwendung benutzbar strukturieren.

## Inhalte
- gemeinsame Shell-/Resource-Partials und area-spezifische Layouts
- Bereichsnavigation
- Dashboard-Grundseiten
- Login / Startlogik
- menüabhängige Sichtbarkeit
- Grundstruktur für Benachrichtigungsanzeige
- zentrales CSS-System für Theme / Shell / Components / Modules

## Ergebnis
- Benutzer landen im richtigen Bereich
- Navigation folgt Bereich und Rolle
- Oberfläche ist sauber vorbereitet
- der UI-Rahmen kann zentral statt verteilt über viele Views gepflegt werden

---

## 6. Phase 4 – Zentrale Benutzer-/Organisationsverwaltung

## Ziel
Die erste echte Fachfunktion der neuen Suite umsetzen.

## Inhalte
- Benutzer anlegen
- Benutzer aktivieren / sperren
- Bereich zuordnen
- Bereichsrolle vergeben
- bereichsbezogene Benutzerlisten
- serverseitige Sichtbarkeit durchsetzen

## Ergebnis
- Wachleiter sieht nur ITW-Benutzer
- Geschäftsführer Verwaltung sieht nur Verwaltungsbenutzer
- Geschäftsführung sieht nur freigegebene Führungsebenenansichten

---

## 7. Phase 5 – ITW-Personal

## Ziel
Die fachliche Grundlage für Intensivtransport schaffen und als zusammenhängenden Mitarbeiterbereich konsolidieren.

## Inhalte
- ITW-Mitarbeiterprofil
- fachliche ITW-Zusatzdaten
- Hauptqualifikation und Zusatzqualifikationen
- allgemeines Mitarbeiterprofil / Stammdaten
- gebündelte Mitarbeiterdetailseite
- Teilbereiche `Übersicht`, `Qualifikationen`, `Stammdaten`
- Mitarbeiterliste als Einstieg in die Mitarbeiterbearbeitung
- Trennung von Identity, allgemeinen Stammdaten und ITW-Fachdaten
- Synchronisierung von Anzeige-relevanten Namensclaims
- gemeinsame Layout-Anzeige des angemeldeten Benutzers auf Basis dieser Claims
- Mitarbeiterdokumente als Teilbereich der Mitarbeiterdetailseite
- Dokumentkategorien für personalbezogene Nachweise und Unterlagen
- Upload / Download von Mitarbeiterdokumenten

## Ergebnis
- ITW-Fachdaten sind von Identity getrennt
- allgemeine Stammdaten sind ebenfalls getrennt modelliert
- Mitarbeiterbearbeitung wächst nicht als Sammlung loser Einzelmasken
- Bereichslayouts können den angemeldeten Benutzer konsistent anzeigen
- Mitarbeiterdokumente sind am bestehenden Mitarbeiter-Flow angedockt
- Dienstplan und Einsatz können später auf einer sauberen Personalbasis aufbauen

---

## 8. Phase 5.5 – Kontosicherheit / Passwort-Reset-Workflow

## Ziel
Kontosicherheit fachlich sauber in die zentrale Plattform integrieren, ohne sie in Fachmodule auszulagern.

## Inhalte
- `Passwort vergessen` im zentralen Login
- Reset-Anfrage mit Benutzername, Vorname und Nachname
- bereichsbezogene Sichtbarkeit offener Passwort-Reset-Anfragen
- Badge in der jeweiligen Bereichsnavigation
- Vergabe eines temporären Passworts durch zuständige Leitung
- Pflicht zur Passwortänderung beim nächsten Login
- Pflicht zur Passwortänderung auch nach initialem Passwort bei Benutzeranlage

## Ergebnis
- Passwort-Reset bleibt zentral, aber bereichsbezogen bearbeitbar
- zuständige Leitungen sehen offene Anfragen im eigenen Bereich
- Benutzer mit initialem oder temporärem Passwort müssen dieses selbst ändern
- Kontosicherheit bleibt von Fachmodulen getrennt

---

## 9. Phase 6 – Dienstplan Grundausbau und manuelle Planungsbasis

## Ziel
Das erste fachliche ITW-Modul aufbauen und den manuellen Wachleiter-Flow tragfähig machen.

## Inhalte
- Planungsperioden
- offene / geschlossene Wunschphase
- offene Perioden anzeigen
- Dienstwünsche abgeben
- gewünschte Dienstanzahl
- Feiertage / Wochenenden im Wunschkalender
- Wachleiter-Periodenübersicht
- Wachleiterkalender
- Tagesplanung im Modal
- manuelle Grundbesetzung
- Krank / Urlaub / Vertretung
- Monatsauswertung
- Buchhaltungs-PDF
- Urlaubsbasis als serverseitige Planungsgrundlage
- Honorarkraft als eigene Beschäftigungsart im Dienstplan-Kontext

## Ergebnis
- Wachleiter kann Perioden steuern
- Mitarbeiter können Wünsche bis zum Ende der Wunschphase abgeben
- Dienstwünsche sind sauber im Modul Dienstplan verortet
- nach Schließen der Wunschphase steht die Planungsgrundlage fest
- manuelle Tagesplanung ist fachlich nutzbar
- Urlaub und Ausfälle greifen bereits in die operative Besetzung ein

---

## 10. Phase 7 – Regelbasierter Autoplan als eigener Flow

## Ziel
Die Planung des Wachleiters durch einen nachvollziehbaren Vorschlags-Flow unterstützen, ohne die manuelle Oberhand des Wachleiters zu verdrängen.

## Inhalte
- eigener Autoplan-Flow statt Assistenten-Zwischenlogik im Tages-Modal
- Vorschau pro planbarem Tag
- Zuweisungsgründe pro Slot
- Konflikterkennung
- Pflichtfälle bei Freelancern
- Priorisierung kritischer Tage
- Änderungsübersicht gegenüber dem aktuellen Plan
- direkter Wechsel in die manuelle Tagesplanung
- Speichern eines Arbeitsstands aus der Vorschau
- klare fachliche Regel:
  - Wachleiter ist der Entscheider
  - Autoplan macht Vorschläge
  - manuelle Wachleiter-Entscheidungen sind führend

## Ergebnis
- der Wachleiter erhält schnell einen belastbaren Planvorschlag
- Autoplan und manuelle Tagesplanung greifen ineinander
- der Wachleiter kann Konflikte und Sonderfälle gezielt manuell lösen
- der Wachleiter behält bei Einzelentscheidungen die Oberhand
- der Autoplan muss sich manuellen Wachleiter-Entscheidungen fügen

---

## 11. Phase 8 – Freigabe, Stabilisierung und spätere Verfeinerung

## Ziel
Den Dienstplan fachlich absichern und danach kontrolliert weiter verfeinern.

## Inhalte
- klare Trennung zwischen Arbeitsstand und freigegebenem Plan
- Freigabe / Veröffentlichung
- vollständige Besetzungsprüfung vor Freigabe
- slotbezogene operative Pflege bei Ausfall / Vertretung
- weitere UI-Schärfung im Wachleiter-Flow
- Testausbau
- optional spätere lernende Verfeinerungen innerhalb der Hard Rules
- optional spätere zusätzliche Benachrichtigungen rund um Planungsstatus

## Ergebnis
- Arbeitsstand und verbindlicher Plan sind klar getrennt
- Freigabe ist fachlich belastbar abgesichert
- operative Änderungen bleiben beim Wachleiter
- der Dienstplan kann kontrolliert weiter wachsen, ohne Blackbox zu werden

---

## 12. Phase 9 – Einsatzmodul

## Ziel
Das zweite ITW-Fachmodul ergänzen.

## Inhalte
- Einsatzstammdaten
- Besetzungen
- Einsatzstatus
- Übersichten
- spätere Dokumentationsgrundlagen

## Ergebnis
- Einsatzlogik bleibt eigenes Modul
- keine Vermischung mit Dienstplan

---

## 13. Phase 10 – Erweiterung und Konsolidierung

## Ziel
System stabilisieren und ausbauen.

## Inhalte
- Auswertungen
- zusätzliche Benachrichtigungen
- zusätzliche Verwaltungsfunktionen
- zusätzliche ITW-Fachmodule
- technische Härtung
- Testausbau
- größeres Refactoring gewachsener Module wie `ITW.Dienstplan`, wenn der fachliche Stand stabil ist
- kontrollierte Frontend-Konsolidierung im Web-Projekt:
  - zentrale CSS-Dateien
  - zentrale JS-Dateien
  - einheitliche Designlinie
  - bereichsabhängige Farbführung
  - schrittweises Entfernen von Inline-CSS und Inline-JS aus Views
- spätere Navigations- und Design-Bereinigung, z. B. Reduzierung dienstplanspezifischer Logik in der `AreaNavigation`

## Ergebnis
- Suite wächst kontrolliert weiter
- Grundarchitektur bleibt stabil
- größere Refactorings erfolgen erst nach fachlicher Stabilisierung

---

## 14. Reihenfolge-Regel

Folgende Dinge werden bewusst **nicht** zuerst gebaut:

- komplexe automatische Komplettplanung vor einer stabilen manuellen Planungsbasis
- Einsatzmodule vor sauberer Personal- und Rollenbasis
- beliebige UI-Masken ohne klare Ownership
- Modulerweiterungen ohne definierte Projektgrenzen
- überfrachtete Kalenderoberflächen statt klarer Führungsoberflächen für den Wachleiter
- große Strukturumbauten mitten in einer noch nicht stabilisierten Fachlogik

---

## 15. Zusammenfassung

- zuerst Plattform, Bereiche, Benutzer und Personalbasis
- dann tragfähige manuelle Dienstplanung
- danach ein eigener regelbasierter Autoplan als Vorschlagswerkzeug
- anschließend Freigabe, Stabilisierung und kontrollierte Verfeinerung
- parallel dazu kontrollierte Frontend-Konsolidierung im Web-Projekt
- erst später größere Refactorings, Navigationsbereinigung und weitere Module

Damit bleibt der Ausbau kontrolliert, nachvollziehbar und passend zur tatsächlichen Arbeitsweise im Intensivtransport.