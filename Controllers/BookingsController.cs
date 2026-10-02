using coworking.Data;
using coworking.Models;
using coworking.Mvc;
using coworking.Services;
using coworking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace coworking.Controllers;

public class BookingsController(
    CoworkingDbContext db,
    IBookingPriceService prices,
    IBookingSchedule schedule,
    BookingFormPrep forms) : Controller
{
    public async Task<IActionResult> Index(BookingIndexQuery query, CancellationToken cancellationToken)
    {
        var bookings = db.Bookings.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Status))
            bookings = bookings.Where(booking => booking.Status == query.Status);
        if (query.RoomId is int roomId)
            bookings = bookings.Where(booking => booking.RoomId == roomId);
        if (query.ClientId is int clientId)
            bookings = bookings.Where(booking => booking.ClientId == clientId);
        if (query.Date is DateOnly date)
        {
            var from = date.ToDateTime(TimeOnly.MinValue);
            var to = from.AddDays(1);
            bookings = bookings.Where(booking => booking.StartTime < to && booking.EndTime > from);
        }

        var raw = await bookings
            .OrderByDescending(booking => booking.StartTime)
            .Select(booking => new
            {
                booking.Id,
                booking.ClientId,
                ClientName = booking.Client.FullName,
                booking.RoomId,
                RoomName = booking.Room.Name,
                booking.Room.HourlyRate,
                booking.StartTime,
                booking.EndTime,
                booking.Status
            })
            .ToListAsync(cancellationToken);

        var rooms = await db.Rooms.AsNoTracking()
            .OrderBy(room => room.Name)
            .Select(room => new { room.Id, room.Name })
            .ToListAsync(cancellationToken);
        var clients = await db.Clients.AsNoTracking()
            .OrderBy(client => client.FullName)
            .Select(client => new { client.Id, client.FullName })
            .ToListAsync(cancellationToken);

        return View(new BookingIndexViewModel
        {
            Query = query,
            Bookings = raw.Select(booking => new BookingRowViewModel
            {
                Id = booking.Id,
                ClientId = booking.ClientId,
                ClientName = booking.ClientName,
                RoomId = booking.RoomId,
                RoomName = booking.RoomName,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Status = booking.Status,
                Hours = prices.Hours(booking.StartTime, booking.EndTime),
                RoomCost = prices.RoomCost(booking.HourlyRate, booking.StartTime, booking.EndTime)
            }).ToList(),
            Statuses = BookingStatuses.All
                .Select(status => new SelectListItem(BookingStatuses.Title(status), status, status == query.Status))
                .ToList(),
            Rooms = rooms
                .Select(room => new SelectListItem(room.Name, room.Id.ToString(), room.Id == query.RoomId))
                .ToList(),
            Clients = clients
                .Select(client => new SelectListItem(client.FullName, client.Id.ToString(), client.Id == query.ClientId))
                .ToList()
        });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var booking = await Load(id, cancellationToken);
        return booking is null ? this.Missing() : View(ToDetails(booking));
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var start = DateTime.Today.AddDays(1).AddHours(10);
        var model = new BookingFormViewModel
        {
            StartTime = start,
            EndTime = start.AddHours(2)
        };
        await forms.FillAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingFormViewModel model, CancellationToken cancellationToken)
    {
        var lines = Chosen(model);
        await Validate(model, lines, null, BookingStatuses.New, cancellationToken);
        if (!ModelState.IsValid)
        {
            await forms.FillAsync(model, cancellationToken);
            return View(model);
        }

        var booking = new Booking
        {
            ClientId = model.ClientId,
            RoomId = model.RoomId,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            Status = BookingStatuses.New,
            CreatedAt = DateTime.Now,
            BookingServices = lines.Select(line => new BookingService
            {
                ServiceId = line.ServiceId,
                Quantity = line.Quantity
            }).ToList()
        };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Message"] = "Бронирование создано.";
        return RedirectToAction(nameof(Details), new { id = booking.Id });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var booking = await Load(id, cancellationToken);
        if (booking is null)
            return this.Missing();

        var model = new BookingFormViewModel
        {
            Id = booking.Id,
            ClientId = booking.ClientId,
            RoomId = booking.RoomId,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Services = booking.BookingServices.Select(line => new BookingServiceInput
            {
                ServiceId = line.ServiceId,
                Selected = true,
                Quantity = line.Quantity
            }).ToList()
        };
        await forms.FillAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookingFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
            return this.Missing();

        var booking = await db.Bookings
            .Include(item => item.BookingServices)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (booking is null)
            return this.Missing();

        var lines = Chosen(model);
        await Validate(model, lines, id, booking.Status, cancellationToken);
        if (!ModelState.IsValid)
        {
            await forms.FillAsync(model, cancellationToken);
            return View(model);
        }

        booking.ClientId = model.ClientId;
        booking.RoomId = model.RoomId;
        booking.StartTime = model.StartTime;
        booking.EndTime = model.EndTime;
        db.BookingServices.RemoveRange(booking.BookingServices);
        foreach (var line in lines)
        {
            booking.BookingServices.Add(new BookingService
            {
                ServiceId = line.ServiceId,
                Quantity = line.Quantity
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        TempData["Message"] = "Бронирование сохранено.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, string status, CancellationToken cancellationToken)
    {
        var booking = await db.Bookings.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (booking is null)
            return this.Missing();

        if (!BookingStatuses.All.Contains(status))
        {
            TempData["Error"] = "Такого статуса нет.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!BookingStatuses.CanChange(booking.Status, status))
        {
            TempData["Error"] = $"Нельзя перевести бронирование из «{BookingStatuses.Title(booking.Status)}» в «{BookingStatuses.Title(status)}».";
            return RedirectToAction(nameof(Details), new { id });
        }

        booking.Status = status;
        await db.SaveChangesAsync(cancellationToken);
        TempData["Message"] = "Статус обновлён.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<Booking?> Load(int id, CancellationToken cancellationToken)
    {
        return await db.Bookings.AsNoTracking()
            .Include(booking => booking.Room)
            .Include(booking => booking.Client)
            .Include(booking => booking.BookingServices)
            .ThenInclude(line => line.Service)
            .FirstOrDefaultAsync(booking => booking.Id == id, cancellationToken);
    }

    private BookingDetailsViewModel ToDetails(Booking booking)
    {
        var priced = booking.BookingServices.Select(line => new PricedLine(line.Service.Price, line.Quantity));
        return new BookingDetailsViewModel
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            RoomName = booking.Room.Name,
            RoomType = booking.Room.RoomType,
            Capacity = booking.Room.Capacity,
            HourlyRate = booking.Room.HourlyRate,
            RoomIsActive = booking.Room.IsActive,
            ClientId = booking.ClientId,
            ClientName = booking.Client.FullName,
            Email = booking.Client.Email,
            Phone = booking.Client.Phone,
            Company = booking.Client.Company,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            Hours = prices.Hours(booking.StartTime, booking.EndTime),
            RoomCost = prices.RoomCost(booking.Room.HourlyRate, booking.StartTime, booking.EndTime),
            ServicesCost = prices.ServicesCost(priced),
            Total = prices.Total(booking.Room.HourlyRate, booking.StartTime, booking.EndTime, priced),
            Services = booking.BookingServices
                .OrderBy(line => line.Service.Name)
                .Select(line => new BookingServiceRowViewModel
                {
                    ServiceId = line.ServiceId,
                    Name = line.Service.Name,
                    Price = line.Service.Price,
                    Quantity = line.Quantity,
                    LineTotal = prices.ServicesCost([new PricedLine(line.Service.Price, line.Quantity)])
                })
                .ToList(),
            NextStatuses = BookingStatuses.All.Where(status => BookingStatuses.CanChange(booking.Status, status)).ToList()
        };
    }

    private static List<ServiceLineDraft> Chosen(BookingFormViewModel model)
    {
        return model.Services
            .Where(line => line.Selected)
            .Select(line => new ServiceLineDraft { ServiceId = line.ServiceId, Quantity = line.Quantity })
            .ToList();
    }

    private async Task Validate(
        BookingFormViewModel model,
        List<ServiceLineDraft> lines,
        int? bookingId,
        string status,
        CancellationToken cancellationToken)
    {
        foreach (var error in await schedule.ValidateAsync(new BookingDraft
        {
            BookingId = bookingId,
            ClientId = model.ClientId,
            RoomId = model.RoomId,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            Status = status
        }, cancellationToken))
            ModelState.AddModelError(string.Empty, error.Message);

        foreach (var error in await schedule.ValidateLinesAsync(lines, cancellationToken))
            ModelState.AddModelError(string.Empty, error.Message);
    }
}
