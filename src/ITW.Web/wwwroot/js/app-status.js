(function () {
    "use strict";

    var statusSelector = "[data-app-status]";
    var closeSelector = "[data-app-status-close]";

    function closeStatus(statusElement) {
        if (!statusElement || statusElement.dataset.appStatusClosing === "true") {
            return;
        }

        statusElement.dataset.appStatusClosing = "true";
        statusElement.classList.add("is-closing");

        window.setTimeout(function () {
            if (statusElement.parentNode) {
                statusElement.parentNode.removeChild(statusElement);
            }
        }, 220);
    }

    function setupStatus(statusElement) {
        if (!statusElement || statusElement.dataset.appStatusReady === "true") {
            return;
        }

        statusElement.dataset.appStatusReady = "true";

        var timeoutMilliseconds = window.parseInt(statusElement.dataset.appStatusAutohideMs || "5000", 10);
        if (window.isNaN(timeoutMilliseconds) || timeoutMilliseconds < 1000) {
            timeoutMilliseconds = 5000;
        }

        statusElement.style.setProperty("--app-status-autohide-ms", timeoutMilliseconds + "ms");

        if (statusElement.dataset.appStatusAutohide !== "true") {
            return;
        }

        window.setTimeout(function () {
            closeStatus(statusElement);
        }, timeoutMilliseconds);
    }

    function initializeStatuses() {
        document.querySelectorAll(statusSelector).forEach(setupStatus);
    }

    document.addEventListener("click", function (event) {
        var closeButton = event.target.closest(closeSelector);
        if (!closeButton) {
            return;
        }

        closeStatus(closeButton.closest(statusSelector));
    });

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializeStatuses);
    } else {
        initializeStatuses();
    }
})();