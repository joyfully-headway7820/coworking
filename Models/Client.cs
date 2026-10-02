namespace coworking.Models;

public class Client
{
    public int Id { get; set; }

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Phone { get; set; } = "";

    public string? Company { get; set; }

    public ICollection<Booking> Bookings { get; set; } = [];
}
