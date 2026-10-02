using coworking.Data;
using coworking.Models;
using Microsoft.EntityFrameworkCore;

namespace coworking.Services;

public class BookingSchedule(CoworkingDbContext db) : IBookingSchedule
{
    public async Task<IReadOnlyList<RuleViolation>> ValidateAsync(BookingDraft draft, CancellationToken cancellationToken)
    {
        var errors = new List<RuleViolation>();
        if (draft.StartTime == default || draft.EndTime == default)
            errors.Add(RuleViolation.Invalid("Укажите время начала и окончания."));
        else if (draft.StartTime >= draft.EndTime)
            errors.Add(RuleViolation.Invalid("Время начала должно быть раньше времени окончания."));

        var occupies = BookingStatuses.OccupiesRoom(draft.Status);
        if (occupies && draft.StartTime != default && draft.StartTime < DateTime.Now)
            errors.Add(RuleViolation.Invalid("Нельзя бронировать время в прошлом."));

        var clientExists = await db.Clients.AnyAsync(client => client.Id == draft.ClientId, cancellationToken);
        if (!clientExists)
            errors.Add(RuleViolation.NotFound("Клиент не найден."));

        var room = await db.Rooms.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == draft.RoomId, cancellationToken);
        if (room is null)
            errors.Add(RuleViolation.NotFound("Помещение не найдено."));
        else if (occupies && !room.IsActive)
            errors.Add(RuleViolation.Conflict("Помещение недоступно для бронирования."));

        if (room is not null && occupies && draft.StartTime < draft.EndTime && draft.StartTime != default)
        {
            var overlaps = await db.Bookings.AnyAsync(booking =>
                booking.RoomId == draft.RoomId
                && (draft.BookingId == null || booking.Id != draft.BookingId)
                && BookingStatuses.Occupying.Contains(booking.Status)
                && booking.StartTime < draft.EndTime
                && booking.EndTime > draft.StartTime, cancellationToken);
            if (overlaps)
                errors.Add(RuleViolation.Conflict("Помещение уже занято на выбранный интервал."));
        }

        return errors;
    }

    public async Task<IReadOnlyList<RuleViolation>> ValidateLinesAsync(
        IReadOnlyCollection<ServiceLineDraft> lines,
        CancellationToken cancellationToken)
    {
        var errors = new List<RuleViolation>();
        if (lines.Count == 0)
            return errors;

        if (lines.GroupBy(line => line.ServiceId).Any(group => group.Count() > 1))
            errors.Add(RuleViolation.Invalid("Одна и та же услуга указана дважды."));

        if (lines.Any(line => line.Quantity < 1))
            errors.Add(RuleViolation.Invalid("Количество услуги должно быть больше нуля."));

        var ids = lines.Select(line => line.ServiceId).Distinct().ToList();
        var found = await db.Services.AsNoTracking()
            .Where(service => ids.Contains(service.Id))
            .Select(service => new { service.Id, service.IsActive })
            .ToListAsync(cancellationToken);

        if (found.Count != ids.Count)
            errors.Add(RuleViolation.NotFound("Услуга не найдена."));

        if (found.Any(service => !service.IsActive))
            errors.Add(RuleViolation.Conflict("Услуга недоступна."));

        return errors;
    }

    public Task<List<Room>> AvailableRoomsAsync(DateTime start, DateTime end, int capacity, CancellationToken cancellationToken)
    {
        var busyRoomIds = db.Bookings
            .Where(booking =>
                BookingStatuses.Occupying.Contains(booking.Status)
                && booking.StartTime < end
                && booking.EndTime > start)
            .Select(booking => booking.RoomId);

        return db.Rooms.AsNoTracking()
            .Where(room => room.IsActive && room.Capacity >= capacity && !busyRoomIds.Contains(room.Id))
            .OrderBy(room => room.Name)
            .ToListAsync(cancellationToken);
    }
}
