document.addEventListener("DOMContentLoaded", function () {
    const modalEl = document.getElementById("tagesplanungModal");

    document.querySelectorAll(".quick-assign-btn").forEach(btn => {
        btn.addEventListener("click", function () {
            const select = document.getElementById(this.dataset.targetSelectId);
            if (select) {
                select.value = this.dataset.userId;
            }
        });
    });

    if (modalEl && modalEl.dataset.autoOpen === "true") {
        const modal = new bootstrap.Modal(modalEl);
        modal.show();
    }
});