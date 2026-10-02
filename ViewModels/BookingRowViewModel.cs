namespace coworking.ViewModels;

public class BookingRowViewModel
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string ClientName { get; set; } = "";

    public int RoomId { get; set; }

    public string RoomName { get; set; } = "";

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = "";

    public decimal Hours { get; set; }

    public decimal RoomCost { get; set; }
}
