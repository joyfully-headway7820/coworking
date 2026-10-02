namespace coworking.ViewModels;

public class RoomDetailsViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string RoomType { get; set; } = "";

    public int Capacity { get; set; }

    public decimal HourlyRate { get; set; }

    public bool IsActive { get; set; }

    public List<BookingLinkRowViewModel> Bookings { get; set; } = [];
}
