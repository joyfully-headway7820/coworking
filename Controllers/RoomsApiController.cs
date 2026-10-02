using coworking.Data;
using coworking.Dtos;
using coworking.Mvc;
using coworking.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace coworking.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsApiController(CoworkingDbContext db, IBookingSchedule schedule) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RoomDto>>> List(CancellationToken cancellationToken)
    {
        var rooms = await db.Rooms.AsNoTracking()
            .OrderBy(room => room.Name)
            .Select(room => new RoomDto
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity,
                HourlyRate = room.HourlyRate,
                RoomType = room.RoomType,
                IsActive = room.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(rooms);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var room = await db.Rooms.AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new RoomDto
            {
                Id = item.Id,
                Name = item.Name,
                Capacity = item.Capacity,
                HourlyRate = item.HourlyRate,
                RoomType = item.RoomType,
                IsActive = item.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        return room is null
            ? NotFound(new ApiError { Message = "Помещение не найдено." })
            : Ok(room);
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<RoomDto>>> Available(
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end,
        [FromQuery] int? capacity,
        CancellationToken cancellationToken)
    {
        if (start is null || end is null || capacity is null)
            return BadRequest(new ApiError { Message = "Укажите start, end и capacity." });
        if (capacity < 1)
            return BadRequest(new ApiError { Message = "Вместимость должна быть больше нуля." });
        if (start >= end)
            return BadRequest(new ApiError { Message = "Время начала должно быть раньше времени окончания." });

        var rooms = await schedule.AvailableRoomsAsync(start.Value, end.Value, capacity.Value, cancellationToken);
        return Ok(rooms.Select(room => new RoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Capacity = room.Capacity,
            HourlyRate = room.HourlyRate,
            RoomType = room.RoomType,
            IsActive = room.IsActive
        }).ToList());
    }
}
