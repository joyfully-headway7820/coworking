using coworking.Dtos;
using coworking.Models;
using coworking.Services;

namespace coworking.Mvc;

public static class BookingMapper
{
    public static BookingDto ToDto(Booking booking, IBookingPriceService prices)
    {
        var lines = booking.BookingServices
            .OrderBy(line => line.Service.Name)
            .Select(line => new BookingServiceDto
            {
                ServiceId = line.ServiceId,
                Name = line.Service.Name,
                Price = line.Service.Price,
                Quantity = line.Quantity,
                LineTotal = prices.ServicesCost([new PricedLine(line.Service.Price, line.Quantity)])
            })
            .ToList();
        var priced = lines.Select(line => new PricedLine(line.Price, line.Quantity));

        return new BookingDto
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            RoomName = booking.Room.Name,
            ClientId = booking.ClientId,
            ClientName = booking.Client.FullName,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            Hours = prices.Hours(booking.StartTime, booking.EndTime),
            RoomCost = prices.RoomCost(booking.Room.HourlyRate, booking.StartTime, booking.EndTime),
            ServicesCost = prices.ServicesCost(priced),
            Total = prices.Total(booking.Room.HourlyRate, booking.StartTime, booking.EndTime, priced),
            Services = lines
        };
    }
}
