namespace coworking.Models;

public static class BookingStatuses
{
    public const string New = "New";

    public const string Confirmed = "Confirmed";

    public const string Completed = "Completed";

    public const string Cancelled = "Cancelled";

    public static readonly string[] All = [New, Confirmed, Completed, Cancelled];

    public static readonly string[] Occupying = [New, Confirmed];

    public static bool OccupiesRoom(string status) => Occupying.Contains(status);

    public static bool CanDelete(string status) => status != Completed;

    public static bool CanChange(string from, string to) => (from, to) switch
    {
        (New, Confirmed) => true,
        (New, Cancelled) => true,
        (Confirmed, Completed) => true,
        (Confirmed, Cancelled) => true,
        _ => false
    };

    public static string Title(string status) => status switch
    {
        New => "Новое",
        Confirmed => "Подтверждено",
        Completed => "Завершено",
        Cancelled => "Отменено",
        _ => status
    };
}
