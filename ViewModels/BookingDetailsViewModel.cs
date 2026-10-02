namespace coworking.ViewModels;

public class BookingDetailsViewModel
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public string RoomName { get; set; } = "";

    public string RoomType { get; set; } = "";

    public int Capacity { get; set; }

    public decimal HourlyRate { get; set; }

    public bool RoomIsActive { get; set; }

    public int ClientId { get; set; }

    public string ClientName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Phone { get; set; } = "";

    public string? Company { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public decimal Hours { get; set; }

    public decimal RoomCost { get; set; }

    public decimal ServicesCost { get; set; }

    public decimal Total { get; set; }

    public List<BookingServiceRowViewModel> Services { get; set; } = [];

    public List<string> NextStatuses { get; set; } = [];
}
