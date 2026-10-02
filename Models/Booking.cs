namespace coworking.Models;

public class Booking
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public Room Room { get; set; } = null!;

    public int ClientId { get; set; }

    public Client Client { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = BookingStatuses.New;

    public DateTime CreatedAt { get; set; }

    public ICollection<BookingService> BookingServices { get; set; } = [];
}
