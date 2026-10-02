namespace coworking.ViewModels;

public class BookingIndexQuery
{
    public string? Status { get; set; }

    public int? RoomId { get; set; }

    public int? ClientId { get; set; }

    public DateOnly? Date { get; set; }
}
