using coworking.Data;
using coworking.Models;
using coworking.Services;
using coworking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace coworking.Controllers;

public class AdminController(CoworkingDbContext db, IBookingPriceService prices) : Controller
{
    public async Task<IActionResult> Dashboard(string? status, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var completed = await db.Bookings.AsNoTracking()
            .Include(booking => booking.Room)
            .Include(booking => booking.BookingServices)
            .ThenInclude(line => line.Service)
            .Where(booking => booking.Status == BookingStatuses.Completed)
            .ToListAsync(cancellationToken);

        return View(new DashboardViewModel
        {
            RoomCount = await db.Rooms.CountAsync(cancellationToken),
            ActiveRoomCount = await db.Rooms.CountAsync(room => room.IsActive, cancellationToken),
            ClientCount = await db.Clients.CountAsync(cancellationToken),
            TodayBookingCount = await db.Bookings.CountAsync(booking =>
                booking.StartTime < tomorrow && booking.EndTime > today, cancellationToken),
            NewBookingCount = await db.Bookings.CountAsync(booking => booking.Status == BookingStatuses.New, cancellationToken),
            CompletedTotal = completed.Sum(booking => prices.Total(
                booking.Room.HourlyRate,
                booking.StartTime,
                booking.EndTime,
                booking.BookingServices.Select(line => new PricedLine(line.Service.Price, line.Quantity)))),
            Status = status,
            Statuses = StatusItems(status),
            Bookings = await Rows(status, cancellationToken)
        });
    }

    public async Task<IActionResult> Bookings(string? status, CancellationToken cancellationToken)
    {
        var rows = await Rows(status, cancellationToken);
        return PartialView("_Bookings", rows);
    }

    private async Task<List<DashboardBookingRowViewModel>> Rows(string? status, CancellationToken cancellationToken)
    {
        IQueryable<Booking> bookings = db.Bookings.AsNoTracking()
            .Include(booking => booking.Client)
            .Include(booking => booking.Room)
            .Include(booking => booking.BookingServices)
            .ThenInclude(line => line.Service);

        if (!string.IsNullOrWhiteSpace(status) && BookingStatuses.All.Contains(status))
            bookings = bookings.Where(booking => booking.Status == status);

        var items = await bookings
            .OrderByDescending(booking => booking.StartTime)
            .ToListAsync(cancellationToken);

        return items.Select(booking => new DashboardBookingRowViewModel
        {
            Id = booking.Id,
            ClientName = booking.Client.FullName,
            RoomName = booking.Room.Name,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status,
            Total = prices.Total(
                booking.Room.HourlyRate,
                booking.StartTime,
                booking.EndTime,
                booking.BookingServices.Select(line => new PricedLine(line.Service.Price, line.Quantity)))
        }).ToList();
    }

    private static List<SelectListItem> StatusItems(string? status)
    {
        var items = new List<SelectListItem> { new("Все", "") };
        items.AddRange(BookingStatuses.All.Select(item => new SelectListItem(BookingStatuses.Title(item), item, item == status)));
        return items;
    }
}
