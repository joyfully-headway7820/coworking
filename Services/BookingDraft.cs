namespace coworking.Services;

public sealed class BookingDraft
{
    public int? BookingId { get; init; }

    public int ClientId { get; init; }

    public int RoomId { get; init; }

    public DateTime StartTime { get; init; }

    public DateTime EndTime { get; init; }

    public string Status { get; init; } = "";
}
