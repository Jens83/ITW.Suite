# Konzeptionelles Datenmodell der ITW-Suite

## Zweck dieses Dokuments

Dieses Dokument beschreibt das konzeptionelle Datenmodell der neuen ITW-Suite ohne technische Implementierungsdetails.

Ziel ist eine saubere Trennung zwischen:

- zentraler Identität,
- organisatorischer Zuordnung,
- bereichsspezifischen Fachdaten,
- modulspezifischen Daten.

---

## 1. Grundprinzip

Das Datenmodell folgt dem Prinzip:

**Zentrale Identität + organisatorische Zuordnung + fachliche Profile + modulspezifische Daten**

Es wird bewusst vermieden, alle Informationen in einer einzigen großen Benutzerstruktur zu sammeln.

---

## 2. Zentrale Identität

Die zentrale Identität ist die einzige Quelle für Anmeldung und technische Benutzerkonten.

## Inhalt der zentralen Identität
- Benutzer-ID
- Login-Name
- E-Mail
- Passwort- und Sicherheitsdaten
- Aktiv-/Sperrstatus
- Mehrfaktorstatus
- technische Rollen und Claims, soweit erforderlich

## Gehört nicht in die zentrale Identität
- ITW-Qualifikation
- Verwaltungsprofil
- Dienstwünsche
- Dienstplandaten
- Einsatzdaten
- bereichsspezifische Fachattribute

---

## 3. Organisatorische Zuordnung

Zusätzlich zur zentralen Identität wird jeder Benutzer organisatorisch eingeordnet.

## Kernbegriffe
- Organisationsbereich
- Benutzer-Bereichszuordnung
- Bereichsrolle
- Führungsverantwortung
- Aktivitätsstatus innerhalb des Bereichs

## Ziel
Die organisatorische Einordnung entscheidet darüber:
- zu welchem Bereich ein Benutzer gehört,
- welche Oberfläche sichtbar ist,
- welche Benutzerlisten sichtbar sind,
- welche Verwaltungsfunktionen erlaubt sind.

---

## 4. Bereichsmodell

Zum Start gibt es folgende Bereiche:

- Intensivtransport
- Verwaltung
- Geschäftsführung

Optional bleibt die technische Administration organisatorisch getrennt denkbar.

## Regel für die erste Ausbaustufe
Ein Benutzer besitzt fachlich genau einen primären Bereich.

Das Modell soll jedoch so entworfen werden, dass spätere Sonderfälle nicht grundsätzlich blockiert werden.

---

## 5. Rollenmodell im Datenmodell

Im Datenmodell werden drei Ebenen unterschieden:

### 1. Globale Rollen
Systemweite technische oder übergeordnete Rollen.

### 2. Bereichsrollen
Rollen innerhalb eines konkreten Bereichs.

### 3. Fachliche Merkmale
Keine Berechtigungen, sondern fachliche Eigenschaften.

---

## 6. Fachliche Mitarbeiterprofile

Nicht jeder Benutzer benötigt dieselben Fachdaten.  
Daher werden fachliche Profile getrennt geführt.

## Mögliche Profile
- allgemeines Mitarbeiterprofil
- ITW-Mitarbeiterprofil
- Verwaltungsprofil
- Führungsebene-Profil

## Ziel
- keine überladene zentrale Benutzerstruktur,
- keine vielen leeren Felder,
- saubere Erweiterbarkeit.

---

## 7. ITW-Mitarbeiterprofil

Das ITW-Mitarbeiterprofil beschreibt fachliche Zusatzdaten für Mitarbeiter des Intensivtransports.

## Beispielhafte Inhalte
- Zuordnung zum Bereich Intensivtransport
- fachlicher Status im ITW
- Hauptqualifikation
- Zusatzqualifikationen
- spätere Eignungen und Nachweise
- spätere planungs- oder einsatzrelevante Merkmale

Dieses Profil gehört fachlich zum Bereich Intensivtransport und nicht in die allgemeine Identity.

---

## 8. Qualifikationen

Qualifikationen sind fachliche Merkmale und keine Berechtigungsrollen.

## Beispiele
- Arzt
- Notfallsanitäter
- spätere Zusatzqualifikationen

## Modellierungsregel
Qualifikationen sollen erweiterbar angelegt werden.

Das Modell muss ermöglichen:
- neue Qualifikationstypen,
- zusätzliche Statusinformationen,
- spätere Gültigkeiten oder Nachweise,
- Mehrfachzuordnungen, wenn fachlich erforderlich.

Es soll ausdrücklich vermieden werden, Qualifikationen als starre einzelne Ja/Nein-Felder zu modellieren.

---

## 9. Verwaltungsprofil

Für Benutzer des Bereichs Verwaltung werden verwaltungsspezifische Zusatzdaten getrennt geführt.

Diese dürfen nicht mit ITW-Fachdaten vermischt werden.

---

## 10. Führungsebene-Profil

Für Benutzer der Geschäftsführung bzw. Führungsebene können eigene Zusatzdaten geführt werden, wenn diese fachlich notwendig sind.

Auch diese Daten gehören nicht pauschal in die zentrale Identity.

---

## 11. Modulspezifische Daten im Dienstplan

Das Modul Dienstplan besitzt seine eigenen fachlichen Daten.

## Dazu gehören
- Dienstplanperiode
- Periodenstatus
- Wunschphasenstatus
- Dienstwünsche
- gewünschte Dienstanzahl
- Planungsregeln
- geplante Dienste
- Freigabe- und Veröffentlichungsstatus

## Grundregel
Diese Daten gehören vollständig zum Modul Dienstplan.

Sie werden nicht in der zentralen Benutzerstruktur gespeichert.

---

## 12. Modulspezifische Daten im Einsatz

Das Modul Einsatz besitzt seine eigenen fachlichen Daten.

## Dazu gehören
- Einsatz
- Einsatzstatus
- Einsatzort
- Einsatzzeitraum
- Einsatzbesetzung
- spätere Dokumentation

Auch diese Daten gehören nicht in die zentrale Benutzerstruktur.

---

## 13. Benachrichtigungen

Benachrichtigungen werden als zentrales Querschnittsthema geführt.

## Mögliche Inhalte
- Benachrichtigungsvorlagen
- Empfängerzuordnung
- Kanal
- Status
- Zustellversuche
- In-App-Benachrichtigungen

Benachrichtigungen referenzieren fachliche Ereignisse, besitzen diese aber nicht.

---

## 14. Audit / Nachvollziehbarkeit

Auditdaten werden zentral geführt.

## Typische Inhalte
- wer hat etwas geändert,
- wann wurde etwas geändert,
- in welchem Bereich oder Modul fand die Änderung statt,
- welcher Anwendungsfall wurde ausgeführt.

Auditdaten gehören nicht in einzelne Fachmodule als Sonderlösung, sondern in ein zentrales Audit-Konzept.

---

## 15. Beziehungen auf konzeptioneller Ebene

Das Zielmodell enthält folgende Hauptbeziehungen:

### Benutzerkonto
ist die technische Identität des Benutzers.

### Benutzerkonto -> Bereichszuordnung
ordnet die Identität einem organisatorischen Bereich zu.

### Benutzerkonto -> Bereichsrolle
definiert die Rolle des Benutzers innerhalb seines Bereichs.

### Benutzerkonto -> Fachprofil
verknüpft die Identität mit bereichs- oder fachbezogenen Zusatzdaten.

### ITW-Mitarbeiterprofil -> Qualifikationen
ordnet dem ITW-Profil fachliche Qualifikationen zu.

### Dienstplanmodul -> Benutzerreferenz
verwendet Benutzer-IDs und Profildaten, besitzt diese aber nicht.

### Einsatzmodul -> Benutzerreferenz
verwendet Benutzer-IDs und ggf. Qualifikationsinformationen, besitzt diese aber nicht.

---

## 16. Ownership des Datenmodells

## Zentral / Plattform
Besitzt:
- Benutzerkonto
- Anmeldung
- Sicherheitsdaten
- Bereichszuordnung
- Bereichsrollen
- zentrale Sichtbarkeit
- Audit
- Benachrichtigungen

## Intensivtransport
Besitzt:
- ITW-Mitarbeiterprofil
- ITW-Qualifikationen
- Dienstplan
- Dienstwünsche
- Einsätze

## Verwaltung
Besitzt:
- Verwaltungsprofile
- spätere Verwaltungsdaten

## Geschäftsführung
Besitzt:
- führungsebenenspezifische Zusatzdaten
- Freigabe- und Auswertungslogik, soweit fachlich erforderlich

---

## 17. Erweiterungsregeln

Neue Bereiche, Profile, Qualifikationen oder Module dürfen ergänzt werden, wenn:

- Ownership klar ist,
- keine bestehende Verantwortung vermischt wird,
- Erweiterung nicht zu doppelter Benutzerlogik führt,
- die zentrale Identity schlank bleibt.

---

## 18. Verbotene Modellierungsfehler

Folgende Fehler sind ausdrücklich zu vermeiden:

- alle Fachdaten in die zentrale Benutzerstruktur legen,
- Qualifikationen als Berechtigungsrollen modellieren,
- Dienstwünsche in die Benutzerstruktur verschieben,
- bereichsspezifische Profile vermischen,
- modulbezogene Daten ohne Ownership zentralisieren.

---

## 19. Zusammenfassung

Das Datenmodell der ITW-Suite trennt bewusst zwischen:

- zentraler Identität,
- organisatorischer Zuordnung,
- fachlichen Profilen,
- modulspezifischen Daten.

Diese Trennung ist die Grundlage für saubere Erweiterbarkeit und langfristige Wartbarkeit.