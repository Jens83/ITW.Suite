# ADR 002: Tablet-Tracking statt Fahrzeug-Tracking

## Status

Angenommen

---

## Kontext

Im Fahrzeugmanagement der ITW-Suite wurde zunächst ein GPS-Tracking im Fahrzeugkontext betrachtet.

Der erste technische Ansatz ging davon aus, dass ein Tracking-Gerät einem Fahrzeug zugeordnet wird und Standortdaten anschließend fahrzeugbezogen gespeichert werden.

Im fachlichen Abgleich wurde diese Annahme korrigiert.

Tatsächlich ist die GPS-Quelle nicht das Fahrzeug selbst, sondern das mobile Einsatz-Tablet im Fahrzeug.

Dieses Tablet wird perspektivisch auch für die Einsatzdokumentation genutzt. Es kann außerdem ausgetauscht werden, wenn ein neues Gerät beschafft wird.

Für den operativen Alltag ist entscheidend, wo sich das Einsatztablet befindet. Der Wachleiter weiß organisatorisch, welches Fahrzeug aktuell eingesetzt ist.

---

## Entscheidung

Die ITW-Suite verwendet für das Live-GPS-Tracking ein tabletbasiertes Trackingmodell.

Das bedeutet:

- Das mobile Tablet ist die führende GPS-Entität.
- Standortdaten werden dem Tracking-Gerät zugeordnet.
- Fahrzeuge sind nicht die primäre Entität für das GPS-Tracking.
- Eine aktive Tablet-Fahrzeug-Zuordnung ist für das Live-Tracking nicht erforderlich.
- Die Position des Tablets gilt im operativen Alltag als Standort des aktuell genutzten ITW.

---

## Begründung

Das tabletbasierte Modell passt besser zur tatsächlichen GPS-Quelle.

Es vermeidet unnötige Bedienlogik für den Wachleiter, insbesondere bei:

- Ersatzfahrzeugen,
- Tablet-Tausch,
- Fahrzeugwechseln,
- temporärer Nutzung anderer Fahrzeuge,
- späterer Tablet-Nutzung für Einsatzdokumentation.

Eine feste Tablet-Fahrzeug-Zuordnung würde im aktuellen Betrieb mehr Komplexität erzeugen, ohne für den MVP ausreichend Mehrwert zu liefern.

---

## Konsequenzen

Positive Konsequenzen:

- Das Modell entspricht der echten GPS-Quelle.
- Ein Tablet kann ausgetauscht werden, ohne Fahrzeugdaten umzubauen.
- Es ist keine manuelle Umschaltung zwischen Regel- und Ersatzfahrzeug nötig.
- Der Wachleiter sieht weiterhin den relevanten Live-Standort.
- Die spätere Einsatzdokumentation auf dem Tablet passt zum gleichen Gerätemodell.
- Die Trackinglogik bleibt einfacher und robuster.

Bewusste Einschränkungen:

- Das System weiß aus dem GPS-Tracking allein nicht, welches Fahrzeug physisch genutzt wird.
- Diese Information muss fachlich aus dem Einsatz-/Wachleiterkontext kommen.
- Browserbasiertes Tracking ist abhängig davon, dass die Tracking-Seite aktiv bleibt.
- Hintergrundtracking bei gesperrtem Gerät ist ohne native App oder Kiosk-Konfiguration nur eingeschränkt zuverlässig.

---

## Aktive technische Struktur

Aktive Entities:

- `FahrzeugTrackingGeraet`
- `TrackingGeraetStandortAktuell`
- `TrackingGeraetStandortHistorienpunkt`
- `TrackingGeraetEinrichtungscode`

Aktive Tabellen:

- `FahrzeugTrackingGeraete`
- `TrackingGeraetStandorteAktuell`
- `TrackingGeraetStandortHistorie`

Aktive Services:

- `RegisterTrackingGeraetService`
- `SaveLocationUpdateService`
- `ReadTabletLiveStandortOverviewService`

Aktiver API-Endpunkt:

```text
POST /api/intensivtransport/fahrzeugmanagement/location-update
```

Erwartete Header:

```text
X-Device-Id
X-Api-Key
```

Der Endpunkt prüft:

- DeviceIdentifier
- API-Key
- Aktivstatus des Tracking-Geräts

---

## Nicht mehr aktiver Tracking-Ansatz

Nicht mehr aktiv genutzt werden sollen:

- `FahrzeugTrackingZuordnung`
- `FahrzeugStandorteAktuell`
- `FahrzeugStandortHistorie`

Diese Altstrukturen sollen im aktiven Code nicht mehr verwendet werden.

Alte Tabellen können in der Entwicklungsdatenbank zunächst liegen bleiben, sollen aber fachlich nicht weiter ausgebaut werden.

---

## Sicherheitsfestlegung

Tracking-Geräte werden nicht über Benutzerlogin authentifiziert, sondern über:

- DeviceIdentifier
- API-Key

Der API-Key wird im Klartext nur einmalig nach der Registrierung angezeigt.

Gespeichert wird ausschließlich der Hash.

Wenn der API-Key verloren geht, wird ein neuer API-Key generiert.

Der Wachleiter oder berechtigte Benutzer darf den API-Key nachträglich nicht erneut im Klartext sehen.

---

## Tablet-Seite

Aktuelle Tablet-Seite:

```text
/tablet/tracking
```

Die Seite:

- läuft im Browser des Tablets,
- fragt Standortfreigabe an,
- speichert DeviceIdentifier und API-Key lokal im Browser,
- sendet regelmäßig GPS-Daten an die ITW-Suite,
- kann auf Samsung, Surface oder vergleichbaren Geräten genutzt werden.

Aktuell wird keine native App gebaut.

Für den MVP reicht der Browser-Client.

Surface ohne LTE ist möglich, wenn ein mobiler WLAN-Router genutzt wird.

---

## Bekannte technische Einschränkung

Browserbasiertes Tracking ist nur zuverlässig, solange die Tracking-Seite aktiv ist.

Einschränkungen können auftreten bei:

- gesperrtem Gerät,
- geschlossenem Browser,
- Energiesparmodus,
- fehlender Standortfreigabe,
- instabiler Netzwerkverbindung.

Eine native App oder Kiosk-Konfiguration bleibt eine spätere Option, ist aber aktuell nicht Bestandteil des MVP.

---

## Spätere Vereinfachung

Unter dem Stichwort `Tracking vereinfachen` soll später geprüft werden:

- Wachleiter-freundliche Bedienung ohne technische Eingaben,
- Tablet-Übersicht statt Trackinggeräte-Technik,
- Einrichtungscode / QR-Code statt DeviceIdentifier/API-Key-Eingabe,
- Tablet-Setup über einfache Seite,
- API-Key bleibt technisch im Hintergrund,
- Liveansicht als Tracking-Cockpit.

Diese Vereinfachung wird später umgesetzt und nicht in den aktuellen MVP hineingezogen.

---

## UI-Festlegung

Für Wachleiter- und Tablet-Tracking-Seiten gilt:

- keine Inline-Styles in Views,
- keine Inline-Scripts in Views,
- zentrale CSS-Dateien verwenden,
- zentrale JavaScript-Dateien verwenden,
- vorhandene App-/Modulklassen nutzen,
- keine neuen UI-Sonderwelten erfinden.

Fahrzeugmanagement-spezifische Styles gehören in:

```text
src/ITW.Web/wwwroot/css/ITW.Fahrzeugmanagement.css
```

---

## Verworfene Alternative

Verworfen wurde ein fahrzeugbasiertes Trackingmodell mit aktiver Tablet-Fahrzeug-Zuordnung.

Grund:

- Es erzeugt unnötige Bedienlogik.
- Es passt schlechter zur echten GPS-Quelle.
- Es wäre fehleranfällig bei Fahrzeugwechseln.
- Es erschwert Tablet-Tausch und Geräteeinrichtung.
- Es bringt im aktuellen Betrieb keinen ausreichenden Mehrwert.

---

## Merksatz

Getrackt wird das mobile Einsatz-Tablet, nicht das Fahrzeug.