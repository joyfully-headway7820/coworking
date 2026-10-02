using System.ComponentModel.DataAnnotations;

namespace coworking.Dtos;

public class CreateBookingRequest
{
    [Range(1, int.MaxValue)]
    public int ClientId { get; set; }

    [Range(1, int.MaxValue)]
    public int RoomId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public List<BookingServiceRequest>? Services { get; set; }
}
