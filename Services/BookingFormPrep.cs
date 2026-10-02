using coworking.Data;
using coworking.Formatting;
using coworking.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace coworking.Services;

public class BookingFormPrep(CoworkingDbContext db)
{
    public async Task FillAsync(BookingFormViewModel model, CancellationToken cancellationToken)
    {
        var clients = await db.Clients.AsNoTracking()
            .OrderBy(client => client.FullName)
            .Select(client => new { client.Id, client.FullName })
            .ToListAsync(cancellationToken);
        model.Clients = clients
            .Select(client => new SelectListItem(client.FullName, client.Id.ToString()))
            .ToList();

        var rooms = await db.Rooms.AsNoTracking()
            .OrderBy(room => room.Name)
            .ToListAsync(cancellationToken);
        model.Rooms = rooms
            .Select(room => new SelectListItem(
                $"{room.Name} — {room.RoomType}, {room.Capacity} мест, {Money.Format(room.HourlyRate)}{(room.IsActive ? "" : ", недоступно")}",
                room.Id.ToString()))
            .ToList();

        var posted = model.Services.ToDictionary(line => line.ServiceId);
        var services = await db.Services.AsNoTracking()
            .OrderBy(service => service.Name)
            .ToListAsync(cancellationToken);

        model.Services = services
            .Where(service => service.IsActive || posted.ContainsKey(service.Id))
            .Select(service =>
            {
                posted.TryGetValue(service.Id, out var current);
                return new BookingServiceInput
                {
                    ServiceId = service.Id,
                    Name = service.Name,
                    Price = service.Price,
                    Selected = current?.Selected ?? false,
                    Quantity = current is null ? 1 : current.Quantity
                };
            })
            .ToList();
    }
}
