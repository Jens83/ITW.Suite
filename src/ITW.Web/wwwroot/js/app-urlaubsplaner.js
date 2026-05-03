document.addEventListener("DOMContentLoaded", function () {
    const hiddenVon = document.getElementById("hiddenVon");
    const hiddenBis = document.getElementById("hiddenBis");
    const dateInput = document.getElementById("singleDateRange");
    const form = document.getElementById("vacationForm");
    const statusHost = document.getElementById("urlaubFormStatusHost");

    function renderValidationMessage(message) {
        if (!statusHost) {
            return;
        }

        statusHost.innerHTML = `
            <div class="app-status-stack">
                <div class="app-status app-status--warning"
                     role="alert"
                     aria-live="assertive"
                     data-app-status
                     data-app-status-autohide="false"
                     data-app-status-autohide-ms="3000">
                    <div class="app-status__icon" aria-hidden="true">
                        <i class="bi bi-exclamation-triangle-fill"></i>
                    </div>
                    <div class="app-status__content">
                        <p class="app-status__title">Ungültige Eingabe</p>
                        <p class="app-status__text">${message}</p>
                    </div>
                    <button type="button"
                            class="app-status__close"
                            aria-label="Statusmeldung schließen"
                            data-app-status-close>
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>
            </div>
        `;
    }

    function clearValidationMessage() {
        if (statusHost) {
            statusHost.innerHTML = "";
        }
    }

    if (!dateInput || !hiddenVon || !hiddenBis) {
        return;
    }

    flatpickr(dateInput, {
        mode: "range",
        locale: "de",
        dateFormat: "Y-m-d",
        altInput: true,
        altFormat: "d.m.Y",
        showMonths: 1,
        minDate: `${dateInput.dataset.year ?? ""}-01-01`,
        maxDate: `${dateInput.dataset.year ?? ""}-12-31`,
        onChange: function (selectedDates, dateStr, instance) {
            hiddenVon.value = "";
            hiddenBis.value = "";
            dateInput.classList.remove("is-invalid");
            clearValidationMessage();

            if (selectedDates.length === 2) {
                hiddenVon.value = instance.formatDate(selectedDates[0], "Y-m-d");
                hiddenBis.value = instance.formatDate(selectedDates[1], "Y-m-d");
            }
        }
    });

    if (form) {
        form.addEventListener("submit", function (e) {
            if (!hiddenVon.value || !hiddenBis.value || hiddenVon.value.startsWith("0001") || hiddenBis.value.startsWith("0001")) {
                e.preventDefault();
                dateInput.classList.add("is-invalid");
                renderValidationMessage("Bitte wählen Sie einen gültigen Zeitraum mit Start- und Enddatum im Kalender aus.");
                dateInput.focus();
                return false;
            }

            clearValidationMessage();
        });
    }
});