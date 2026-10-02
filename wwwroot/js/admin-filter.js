function initAdminFilter() {
  const form = document.getElementById("booking-filter");
  const panel = document.getElementById("bookings-panel");
  if (!form || !panel)
    return;

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const status = new FormData(form).get("status") ?? "";
    const button = form.querySelector("button");
    if (button)
      button.disabled = true;

    try {
      const response = await fetch(`/Admin/Bookings?status=${encodeURIComponent(String(status))}`, {
        headers: { "X-Requested-With": "XMLHttpRequest" }
      });
      if (!response.ok) {
        panel.textContent = "Не удалось обновить список.";
        return;
      }
      panel.innerHTML = await response.text();
    } catch {
      panel.textContent = "Сервер не ответил.";
    } finally {
      if (button)
        button.disabled = false;
    }
  });
}

initAdminFilter();
