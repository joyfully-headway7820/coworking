namespace coworking.ViewModels;

public class DashboardBookingRowViewModel
{
    public int Id { get; set; }

    public string ClientName { get; set; } = "";

    public string RoomName { get; set; } = "";

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = "";

    public decimal Total { get; set; }
}
