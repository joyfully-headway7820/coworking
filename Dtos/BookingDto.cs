namespace coworking.Dtos;

public class BookingDto
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public string RoomName { get; set; } = "";

    public int ClientId { get; set; }

    public string ClientName { get; set; } = "";

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public decimal Hours { get; set; }

    public decimal RoomCost { get; set; }

    public decimal ServicesCost { get; set; }

    public decimal Total { get; set; }

    public List<BookingServiceDto> Services { get; set; } = [];
}
