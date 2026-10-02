using coworking.Data;
using coworking.Dtos;
using coworking.Models;
using coworking.Mvc;
using coworking.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace coworking.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsApiController(
    CoworkingDbContext db,
    IBookingPriceService prices,
    IBookingSchedule schedule) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BookingDto>>> List(CancellationToken cancellationToken)
    {
        var bookings = await LoadQuery().OrderByDescending(booking => booking.StartTime).ToListAsync(cancellationToken);
        return Ok(bookings.Select(booking => BookingMapper.ToDto(booking, prices)).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var booking = await LoadQuery().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return booking is null
            ? NotFound(new ApiError { Message = "Бронирование не найдено." })
            : Ok(BookingMapper.ToDto(booking, prices));
    }

    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var lines = (request.Services ?? [])
            .Select(line => new ServiceLineDraft { ServiceId = line.ServiceId, Quantity = line.Quantity })
            .ToList();

        var violations = new List<RuleViolation>();
        violations.AddRange(await schedule.ValidateAsync(new BookingDraft
        {
            ClientId = request.ClientId,
            RoomId = request.RoomId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = BookingStatuses.New
        }, cancellationToken));
        violations.AddRange(await schedule.ValidateLinesAsync(lines, cancellationToken));
        if (violations.Count > 0)
            return ApiResults.FromViolations(violations);

        var booking = new Booking
        {
            ClientId = request.ClientId,
            RoomId = request.RoomId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
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

        var created = await LoadQuery().FirstAsync(item => item.Id == booking.Id, cancellationToken);
        var dto = BookingMapper.ToDto(created, prices);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, dto);
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<BookingDto>> UpdateStatus(int id, UpdateBookingStatusRequest request, CancellationToken cancellationToken)
    {
        if (!BookingStatuses.All.Contains(request.Status))
            return BadRequest(new ApiError { Message = "Такого статуса нет." });

        var booking = await db.Bookings.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (booking is null)
            return NotFound(new ApiError { Message = "Бронирование не найдено." });

        if (booking.Status == request.Status)
            return BadRequest(new ApiError { Message = "Бронирование уже в этом статусе." });

        if (!BookingStatuses.CanChange(booking.Status, request.Status))
            return Conflict(new ApiError
            {
                Message = $"Нельзя перевести бронирование из «{BookingStatuses.Title(booking.Status)}» в «{BookingStatuses.Title(request.Status)}»."
            });

        booking.Status = request.Status;
        await db.SaveChangesAsync(cancellationToken);

        var updated = await LoadQuery().FirstAsync(item => item.Id == id, cancellationToken);
        return Ok(BookingMapper.ToDto(updated, prices));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var booking = await db.Bookings.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (booking is null)
            return NotFound(new ApiError { Message = "Бронирование не найдено." });

        if (!BookingStatuses.CanDelete(booking.Status))
            return Conflict(new ApiError { Message = "Завершённое бронирование удалять нельзя." });

        db.Bookings.Remove(booking);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private IQueryable<Booking> LoadQuery()
    {
        return db.Bookings.AsNoTracking()
            .Include(booking => booking.Room)
            .Include(booking => booking.Client)
            .Include(booking => booking.BookingServices)
            .ThenInclude(line => line.Service);
    }
}
