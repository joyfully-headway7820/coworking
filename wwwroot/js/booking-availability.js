function initBookingAvailability() {
  const button = document.getElementById("check-availability");
  const result = document.getElementById("availability-result");
  const capacityInput = document.getElementById("needed-capacity");
  const startInput = document.getElementById("StartTime");
  const endInput = document.getElementById("EndTime");
  const roomSelect = document.getElementById("RoomId");

  function note(text) {
    const paragraph = document.createElement("p");
    paragraph.className = "flash-error";
    paragraph.textContent = text;
    return paragraph;
  }

  if (!button || !result || !capacityInput || !startInput || !endInput || !roomSelect)
    return;

  button.addEventListener("click", async () => {
    result.replaceChildren();
    const start = startInput.value;
    const end = endInput.value;
    const capacity = capacityInput.value;
    if (!start || !end || !capacity) {
      result.append(note("Укажите начало, окончание и вместимость."));
      return;
    }

    button.disabled = true;
    try {
      const params = new URLSearchParams({ start, end, capacity });
      const response = await fetch(`/api/rooms/available?${params}`);
      const payload = await response.json();
      if (!response.ok) {
        result.append(note(payload.message || "Не удалось проверить доступность."));
        return;
      }
      if (!Array.isArray(payload) || payload.length === 0) {
        result.append(note("Свободных помещений нет."));
        return;
      }

      const list = document.createElement("ul");
      payload.forEach((room) => {
        const item = document.createElement("li");
        const title = document.createElement("span");
        title.textContent = `${room.name} — ${room.roomType}, ${room.capacity} мест, ${room.hourlyRate} ₽/ч`;
        const choose = document.createElement("button");
        choose.type = "button";
        choose.textContent = "Выбрать";
        choose.addEventListener("click", () => {
          roomSelect.value = String(room.id);
          result.querySelectorAll("li").forEach((row) => row.classList.remove("is-selected"));
          item.classList.add("is-selected");
        });
        item.append(title, choose);
        list.append(item);
      });
      result.append(list);
    } catch {
      result.append(note("Сервер не ответил."));
    } finally {
      button.disabled = false;
    }
  });
}

initBookingAvailability();
