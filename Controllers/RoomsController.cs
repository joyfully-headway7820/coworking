using coworking.Data;
using coworking.Models;
using coworking.Mvc;
using coworking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace coworking.Controllers;

public class RoomsController(CoworkingDbContext db) : Controller
{
    public async Task<IActionResult> Index(RoomIndexQuery query, CancellationToken cancellationToken)
    {
        var rooms = db.Rooms.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.RoomType))
            rooms = rooms.Where(room => room.RoomType == query.RoomType);
        if (query.MinCapacity is int minCapacity)
            rooms = rooms.Where(room => room.Capacity >= minCapacity);
        if (query.MaxPrice is decimal maxPrice)
            rooms = rooms.Where(room => room.HourlyRate <= maxPrice);
        if (query.ActiveOnly)
            rooms = rooms.Where(room => room.IsActive);

        var types = await db.Rooms.AsNoTracking()
            .Select(room => room.RoomType)
            .Distinct()
            .OrderBy(type => type)
            .ToListAsync(cancellationToken);

        var rows = await rooms
            .OrderBy(room => room.Name)
            .Select(room => new RoomRowViewModel
            {
                Id = room.Id,
                Name = room.Name,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                HourlyRate = room.HourlyRate,
                IsActive = room.IsActive
            })
            .ToListAsync(cancellationToken);

        return View(new RoomIndexViewModel
        {
            Query = query,
            Rooms = rows,
            RoomTypes = types
                .Select(type => new SelectListItem(type, type, type == query.RoomType))
                .ToList()
        });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var room = await db.Rooms.AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new RoomDetailsViewModel
            {
                Id = item.Id,
                Name = item.Name,
                RoomType = item.RoomType,
                Capacity = item.Capacity,
                HourlyRate = item.HourlyRate,
                IsActive = item.IsActive,
                Bookings = item.Bookings
                    .OrderByDescending(booking => booking.StartTime)
                    .Select(booking => new BookingLinkRowViewModel
                    {
                        Id = booking.Id,
                        RoomId = booking.RoomId,
                        RoomName = item.Name,
                        ClientId = booking.ClientId,
                        ClientName = booking.Client.FullName,
                        StartTime = booking.StartTime,
                        EndTime = booking.EndTime,
                        Status = booking.Status
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return room is null ? this.Missing() : View(room);
    }

    public IActionResult Create() => View(new RoomFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        db.Rooms.Add(new Room
        {
            Name = model.Name.Trim(),
            Capacity = model.Capacity,
            HourlyRate = model.HourlyRate,
            RoomType = model.RoomType.Trim(),
            IsActive = model.IsActive
        });
        await db.SaveChangesAsync(cancellationToken);
        TempData["Message"] = "Помещение создано.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await Form(id, cancellationToken);
        return model is null ? this.Missing() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RoomFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
            return this.Missing();
        if (!ModelState.IsValid)
            return View(model);

        var room = await db.Rooms.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (room is null)
            return this.Missing();

        room.Name = model.Name.Trim();
        room.Capacity = model.Capacity;
        room.HourlyRate = model.HourlyRate;
        room.RoomType = model.RoomType.Trim();
        room.IsActive = model.IsActive;
        await db.SaveChangesAsync(cancellationToken);
        TempData["Message"] = "Помещение сохранено.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var model = await db.Rooms.AsNoTracking()
            .Where(room => room.Id == id)
            .Select(room => new RoomDeleteViewModel
            {
                Id = room.Id,
                Name = room.Name,
                BookingCount = room.Bookings.Count
            })
            .FirstOrDefaultAsync(cancellationToken);

        return model is null ? this.Missing() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var room = await db.Rooms
            .Include(item => item.Bookings)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (room is null)
            return this.Missing();

        if (room.Bookings.Count > 0)
        {
            TempData["Error"] = "Нельзя удалить помещение: есть связанные бронирования.";
            return View("Delete", new RoomDeleteViewModel
            {
                Id = room.Id,
                Name = room.Name,
                BookingCount = room.Bookings.Count
            });
        }

        db.Rooms.Remove(room);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Message"] = "Помещение удалено.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<RoomFormViewModel?> Form(int id, CancellationToken cancellationToken)
    {
        return await db.Rooms.AsNoTracking()
            .Where(room => room.Id == id)
            .Select(room => new RoomFormViewModel
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity,
                HourlyRate = room.HourlyRate,
                RoomType = room.RoomType,
                IsActive = room.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
