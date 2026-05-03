// Fahrzeugmanagement: Fahrzeugstandort
(function () {
    const leafletCssUrl = "https://unpkg.com/leaflet@1.9.4/dist/leaflet.css";
    const leafletJsUrl = "https://unpkg.com/leaflet@1.9.4/dist/leaflet.js";

    const state = {
        map: null,
        marker: null,
        routeLine: null,
        routeShadow: null,
        initialized: false,
        userHasMovedMap: false,
        pollingTimer: null
    };

    function ensureLeafletCss() {
        const existing = document.querySelector(`link[href="${leafletCssUrl}"]`);

        if (existing) {
            return;
        }

        const link = document.createElement("link");
        link.rel = "stylesheet";
        link.href = leafletCssUrl;
        link.crossOrigin = "";

        document.head.appendChild(link);
    }

    function ensureLeafletScript() {
        return new Promise((resolve, reject) => {
            if (window.L) {
                resolve();
                return;
            }

            const existing = document.querySelector(`script[src="${leafletJsUrl}"]`);

            if (existing) {
                existing.addEventListener("load", resolve);
                existing.addEventListener("error", reject);
                return;
            }

            const script = document.createElement("script");
            script.src = leafletJsUrl;
            script.crossOrigin = "";
            script.onload = resolve;
            script.onerror = reject;

            document.body.appendChild(script);
        });
    }

    function getCssVariable(name, fallback) {
        const value = getComputedStyle(document.documentElement)
            .getPropertyValue(name)
            .trim();

        return value || fallback;
    }

    function parseNumber(value) {
        if (value === null || value === undefined || value === "") {
            return null;
        }

        const parsed = Number(value.toString().replace(",", "."));

        return Number.isFinite(parsed)
            ? parsed
            : null;
    }

    function normalizeRoute(route) {
        if (!Array.isArray(route)) {
            return [];
        }

        return route
            .map(point => ({
                latitude: parseNumber(point.latitude),
                longitude: parseNumber(point.longitude),
                speedKmh: point.speedKmh,
                erfasstAmText: point.erfasstAmText
            }))
            .filter(point => point.latitude !== null && point.longitude !== null);
    }

    function parseRoute(raw) {
        if (!raw) {
            return [];
        }

        try {
            const parsed = JSON.parse(raw);

            return normalizeRoute(parsed);
        } catch {
            return [];
        }
    }

    function createPopupHtml(data) {
        return `
            <div class="itw-tracking-map-popup">
                <strong>Fahrzeugstandort</strong>
                <span>Status: ${data.status || "-"}</span>
                <span>Geschwindigkeit: ${data.speed || "-"}</span>
                <span>Standortzeit: ${data.erfasstAm || "-"}</span>
            </div>
        `;
    }

    function createVehicleIcon(isMoving) {
        const movementClass = isMoving
            ? "is-moving"
            : "is-standing";

        return L.divIcon({
            className: "",
            html: `
                <div class="itw-tracking-vehicle-marker ${movementClass}" aria-hidden="true">
                    <i class="bi bi-car-front-fill"></i>
                </div>
            `,
            iconSize: [46, 46],
            iconAnchor: [23, 23],
            popupAnchor: [0, -22]
        });
    }

    function getInitialData(mapElement) {
        const status = mapElement.dataset.status || "-";

        return {
            latitude: parseNumber(mapElement.dataset.latitude),
            longitude: parseNumber(mapElement.dataset.longitude),
            speed: mapElement.dataset.speed || "-",
            status: status,
            erfasstAm: mapElement.dataset.erfasstAm || "-",
            letzterKontakt: "-",
            distance: "-",
            isOnline: true,
            isMoving: status.toLowerCase().includes("fährt"),
            route: parseRoute(mapElement.dataset.route)
        };
    }

    function mapFahrzeugDtoToData(fahrzeug) {
        if (!fahrzeug || fahrzeug.hatStandort !== true) {
            return null;
        }

        return {
            latitude: parseNumber(fahrzeug.latitude),
            longitude: parseNumber(fahrzeug.longitude),
            speed: fahrzeug.speedText || "-",
            status: fahrzeug.bewegungsstatusText || "-",
            erfasstAm: fahrzeug.erfasstAmText || "-",
            letzterKontakt: fahrzeug.letzterKontaktText || "-",
            distance: fahrzeug.gefahreneStreckeText || "-",
            isOnline: fahrzeug.istOnline === true,
            isMoving: fahrzeug.istInBewegung === true,
            route: normalizeRoute(fahrzeug.routeHistorie || [])
        };
    }

    function updateText(selector, value) {
        document
            .querySelectorAll(selector)
            .forEach(element => {
                element.textContent = value || "-";
            });
    }

    function updateBadge(selector, text, cssClass) {
        document
            .querySelectorAll(selector)
            .forEach(element => {
                element.className = `badge ${cssClass}`;
                element.textContent = text || "-";
            });
    }

    function updateStatus(data) {
        updateText("[data-itw-live-status]", data.status);
        updateText("[data-itw-live-speed]", data.speed);
        updateText("[data-itw-live-distance]", data.distance);
        updateText("[data-itw-live-contact]", data.letzterKontakt);

        updateBadge(
            "[data-itw-live-online]",
            data.isOnline ? "Online" : "Offline",
            data.isOnline ? "bg-success" : "bg-warning text-dark");

        updateBadge(
            "[data-itw-live-motion]",
            data.status || "-",
            data.isMoving ? "bg-primary" : "bg-secondary");
    }

    function updateRoute(route) {
        if (!state.map) {
            return;
        }

        const routeLatLngs = normalizeRoute(route)
            .map(point => [point.latitude, point.longitude]);

        const accentColor = getCssVariable("--accent-color", "#0f6cbd");

        if (!state.routeShadow) {
            state.routeShadow = L.polyline([], {
                color: "#93c5fd",
                weight: 11,
                opacity: 0.42,
                lineCap: "round",
                lineJoin: "round"
            }).addTo(state.map);
        }

        if (!state.routeLine) {
            state.routeLine = L.polyline([], {
                color: accentColor,
                weight: 5,
                opacity: 0.96,
                lineCap: "round",
                lineJoin: "round"
            }).addTo(state.map);
        }

        state.routeShadow.setLatLngs(routeLatLngs);
        state.routeLine.setLatLngs(routeLatLngs);
    }

    function createMap(mapElement, data) {
        const currentLatLng = [data.latitude, data.longitude];

        state.map = L.map(mapElement, {
            zoomControl: true,
            attributionControl: true
        });

        state.map.on("dragstart zoomstart", () => {
            state.userHasMovedMap = true;
        });

        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            maxZoom: 19,
            attribution: "&copy; OpenStreetMap-Mitwirkende"
        }).addTo(state.map);

        updateRoute(data.route);

        state.marker = L.marker(currentLatLng, {
            icon: createVehicleIcon(data.isMoving),
            title: "Fahrzeugstandort"
        })
            .addTo(state.map)
            .bindPopup(createPopupHtml(data));

        if (data.route.length >= 2) {
            const bounds = L.latLngBounds(
                data.route.map(point => [point.latitude, point.longitude]));

            bounds.extend(currentLatLng);

            state.map.fitBounds(bounds, {
                padding: [36, 36]
            });
        } else {
            state.map.setView(currentLatLng, 15);
        }

        state.initialized = true;

        window.setTimeout(() => {
            if (state.map) {
                state.map.invalidateSize();
            }
        }, 150);
    }

    function updateMap(mapElement, data) {
        if (!data || data.latitude === null || data.longitude === null || !window.L) {
            return;
        }

        if (!state.initialized) {
            createMap(mapElement, data);
            updateStatus(data);
            return;
        }

        const currentLatLng = [data.latitude, data.longitude];

        updateRoute(data.route);

        state.marker
            .setLatLng(currentLatLng)
            .setIcon(createVehicleIcon(data.isMoving))
            .setPopupContent(createPopupHtml(data));

        updateStatus(data);

        if (
            !state.userHasMovedMap &&
            state.map &&
            !state.map.getBounds().pad(-0.2).contains(currentLatLng)
        ) {
            state.map.panTo(currentLatLng, {
                animate: true,
                duration: 0.8
            });
        }
    }

    async function pollLiveData(pageElement, mapElement) {
        const liveUrl = pageElement.dataset.liveUrl;

        if (!liveUrl) {
            return;
        }

        try {
            const response = await fetch(liveUrl, {
                method: "GET",
                cache: "no-store",
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                return;
            }

            const result = await response.json();

            if (!result || result.success !== true) {
                return;
            }

            const data = mapFahrzeugDtoToData(result.fahrzeug);

            if (!data) {
                return;
            }

            updateMap(mapElement, data);
        } catch {
            // Bewusst still:
            // Bei kurzer Verbindungsstörung soll die Karte sichtbar bleiben.
        }
    }

    function initializePolling(pageElement, mapElement) {
        const pollMs = Number(pageElement.dataset.pollMs || 10000);

        if (!Number.isFinite(pollMs) || pollMs <= 0) {
            return;
        }

        if (state.pollingTimer) {
            window.clearInterval(state.pollingTimer);
        }

        state.pollingTimer = window.setInterval(() => {
            pollLiveData(pageElement, mapElement);
        }, pollMs);
    }

    async function initializeFahrzeugstandort() {
        const pageElement = document.querySelector("[data-itw-tablet-live-page]");
        const mapElement = document.querySelector("[data-itw-tablet-live-map]");

        if (!pageElement || !mapElement) {
            return;
        }

        ensureLeafletCss();

        try {
            await ensureLeafletScript();

            const initialData = getInitialData(mapElement);

            if (initialData.latitude !== null && initialData.longitude !== null) {
                updateMap(mapElement, initialData);
            } else {
                mapElement.innerHTML = `
                    <div class="alert alert-warning m-3">
                        Es liegt noch kein Standort vor.
                    </div>
                `;
            }

            initializePolling(pageElement, mapElement);
        } catch {
            mapElement.innerHTML = `
                <div class="alert alert-warning m-3">
                    Die Karte konnte nicht geladen werden. Bitte Internetverbindung prüfen oder die Seite neu laden.
                </div>
            `;
        }
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializeFahrzeugstandort);
    } else {
        initializeFahrzeugstandort();
    }
})();


