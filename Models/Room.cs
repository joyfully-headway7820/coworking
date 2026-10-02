namespace coworking.Models;

public class Room
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int Capacity { get; set; }

    public decimal HourlyRate { get; set; }

    public string RoomType { get; set; } = "";

    public bool IsActive { get; set; }

    public ICollection<Booking> Bookings { get; set; } = [];
}
