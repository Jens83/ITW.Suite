(function () {
    const page = document.querySelector("[data-itw-tablet-tracking-page]");

    if (!page) {
        return;
    }

    const endpointUrl = page.dataset.endpoint;
    const sendIntervalMs = 10000;

    let watchId = null;
    let sendTimer = null;
    let wakeLock = null;
    let lastPosition = null;
    let previousRawPosition = null;
    let sendCount = 0;
    let isRunning = false;

    const elements = {
        startButton: document.getElementById("startTrackingButton"),
        stopButton: document.getElementById("stopTrackingButton"),
        statusBadge: document.getElementById("trackingStatusBadge"),
        message: document.getElementById("trackingMessage"),
        speedKmh: document.getElementById("speedKmh"),
        sendCount: document.getElementById("sendCount"),
        lastSend: document.getElementById("lastSend"),
        locationStatus: document.getElementById("locationStatus")
    };

    function getDeviceIdentifier() {
        return localStorage.getItem("itwTabletTracking.deviceIdentifier") || "";
    }

    function getApiKey() {
        return localStorage.getItem("itwTabletTracking.apiKey") || "";
    }

    function isAutoStartEnabled() {
        return localStorage.getItem("itwTabletTracking.autoStart") === "true";
    }

    function setMessage(cssClass, text) {
        elements.message.className = `alert ${cssClass}`;
        elements.message.textContent = text;
    }

    function setStatus(text, cssClass) {
        elements.statusBadge.className = `app-pill ${cssClass}`;
        elements.statusBadge.textContent = text;
    }

    function setRunningState(running) {
        isRunning = running;

        elements.startButton.disabled = running;
        elements.stopButton.disabled = !running;

        if (running) {
            setStatus("Live aktiv", "app-pill--success");
        } else {
            setStatus("Bereit", "app-pill--neutral");
        }
    }

    function toRadians(value) {
        return value * (Math.PI / 180);
    }

    function calculateDistanceMeters(a, b) {
        const earthRadiusMeters = 6371000;

        const lat1 = toRadians(a.latitude);
        const lon1 = toRadians(a.longitude);
        const lat2 = toRadians(b.latitude);
        const lon2 = toRadians(b.longitude);

        const deltaLat = lat2 - lat1;
        const deltaLon = lon2 - lon1;

        const haversine =
            Math.pow(Math.sin(deltaLat / 2), 2) +
            Math.cos(lat1) * Math.cos(lat2) * Math.pow(Math.sin(deltaLon / 2), 2);

        const angle = 2 * Math.atan2(Math.sqrt(haversine), Math.sqrt(1 - haversine));

        return earthRadiusMeters * angle;
    }

    function calculateFallbackSpeedKmh(currentRawPosition) {
        if (!previousRawPosition) {
            return 0;
        }

        const seconds = (currentRawPosition.timestamp - previousRawPosition.timestamp) / 1000;

        if (seconds <= 0) {
            return 0;
        }

        const meters = calculateDistanceMeters(
            {
                latitude: previousRawPosition.coords.latitude,
                longitude: previousRawPosition.coords.longitude
            },
            {
                latitude: currentRawPosition.coords.latitude,
                longitude: currentRawPosition.coords.longitude
            });

        return (meters / seconds) * 3.6;
    }

    function createPayload(position) {
        const browserSpeedKmh = position.coords.speed === null
            ? null
            : position.coords.speed * 3.6;

        const fallbackSpeedKmh = calculateFallbackSpeedKmh(position);

        const speedKmh = browserSpeedKmh === null
            ? fallbackSpeedKmh
            : browserSpeedKmh;

        previousRawPosition = position;

        return {
            latitude: Number(position.coords.latitude.toFixed(6)),
            longitude: Number(position.coords.longitude.toFixed(6)),
            speedKmh: Number(Math.max(speedKmh, 0).toFixed(2)),
            erfasstAmUtc: new Date(position.timestamp).toISOString()
        };
    }

    function updateDisplay(payload) {
        elements.speedKmh.textContent = `${payload.speedKmh.toFixed(2)} km/h`;
        elements.locationStatus.textContent = "Standort empfangen";
    }

    function handlePosition(position) {
        lastPosition = createPayload(position);
        updateDisplay(lastPosition);
    }

    function handlePositionError() {
        setStatus("GPS-Fehler", "app-pill--danger");
        setMessage("alert-danger", "Der Standort konnte nicht gelesen werden. Bitte Standortfreigabe prüfen.");
        elements.locationStatus.textContent = "GPS-Fehler";
    }

    async function acquireWakeLock() {
        if (!("wakeLock" in navigator)) {
            return;
        }

        try {
            wakeLock = await navigator.wakeLock.request("screen");
        } catch {
            wakeLock = null;
        }
    }

    async function releaseWakeLock() {
        if (!wakeLock) {
            return;
        }

        try {
            await wakeLock.release();
        } catch {
            // bewusst ignorieren
        } finally {
            wakeLock = null;
        }
    }

    async function sendCurrentPosition() {
        if (!lastPosition) {
            elements.locationStatus.textContent = "Warte auf Standort";
            return;
        }

        const deviceIdentifier = getDeviceIdentifier();
        const apiKey = getApiKey();

        if (!deviceIdentifier || !apiKey) {
            setStatus("Nicht eingerichtet", "app-pill--danger");
            setMessage(
                "alert-danger",
                "Dieses Tablet ist noch nicht eingerichtet. Bitte den QR-Code aus der ITW-Suite scannen.");

            await stopTracking();
            return;
        }

        const response = await fetch(endpointUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-Device-Id": deviceIdentifier,
                "X-Api-Key": apiKey
            },
            body: JSON.stringify(lastPosition)
        });

        if (!response.ok) {
            setStatus("Sende-Fehler", "app-pill--danger");
            setMessage("alert-danger", "Standort konnte nicht gesendet werden. Bitte Verbindung prüfen.");
            return;
        }

        sendCount++;

        elements.sendCount.textContent = sendCount.toString();
        elements.lastSend.textContent = new Date().toLocaleTimeString();

        setStatus("Live aktiv", "app-pill--success");
        setMessage("alert-success", "Tracking läuft. Standort wird automatisch gesendet.");
    }

    async function startTracking() {
        if (isRunning) {
            return;
        }

        if (!getDeviceIdentifier() || !getApiKey()) {
            setStatus("Nicht eingerichtet", "app-pill--danger");
            setMessage(
                "alert-danger",
                "Dieses Tablet ist noch nicht eingerichtet. Bitte den QR-Code aus der ITW-Suite scannen.");
            return;
        }

        if (!navigator.geolocation) {
            setStatus("Kein GPS", "app-pill--danger");
            setMessage("alert-danger", "Dieser Browser unterstützt keine Standortabfrage.");
            return;
        }

        setRunningState(true);
        setMessage("alert-info", "Standortfreigabe wird angefragt. Bitte erlauben.");
        elements.locationStatus.textContent = "Warte auf Standort";

        await acquireWakeLock();

        watchId = navigator.geolocation.watchPosition(
            handlePosition,
            handlePositionError,
            {
                enableHighAccuracy: true,
                timeout: 20000,
                maximumAge: 0
            });

        sendTimer = window.setInterval(async () => {
            try {
                await sendCurrentPosition();
            } catch {
                setStatus("Sende-Fehler", "app-pill--danger");
                setMessage("alert-danger", "Fehler beim automatischen Senden.");
            }
        }, sendIntervalMs);

        window.setTimeout(async () => {
            try {
                await sendCurrentPosition();
            } catch {
                // bewusst still
            }
        }, 2500);
    }

    async function stopTracking() {
        if (watchId !== null) {
            navigator.geolocation.clearWatch(watchId);
            watchId = null;
        }

        if (sendTimer !== null) {
            window.clearInterval(sendTimer);
            sendTimer = null;
        }

        await releaseWakeLock();

        setRunningState(false);
    }

    document.addEventListener("visibilitychange", async () => {
        if (document.visibilityState === "visible" && isRunning && !wakeLock) {
            await acquireWakeLock();
        }
    });

    elements.startButton.addEventListener("click", startTracking);
    elements.stopButton.addEventListener("click", stopTracking);

    setRunningState(false);

    if (!getDeviceIdentifier() || !getApiKey()) {
        setStatus("Nicht eingerichtet", "bg-danger");
        setMessage(
            "alert-danger",
            "Dieses Tablet ist noch nicht eingerichtet. Bitte den QR-Code aus der ITW-Suite scannen.");
        return;
    }

    setStatus("Bereit", "app-pill--neutral");
    setMessage("alert-success", "Tablet ist eingerichtet. Tracking kann gestartet werden.");

    if (isAutoStartEnabled()) {
        window.setTimeout(() => {
            startTracking();
        }, 750);
    }
})();