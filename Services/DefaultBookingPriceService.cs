namespace coworking.Services;

public class DefaultBookingPriceService : IBookingPriceService
{
    public decimal Hours(DateTime start, DateTime end)
    {
        if (end <= start)
            return 0;

        return (decimal)(end - start).TotalHours;
    }

    public decimal RoomCost(decimal hourlyRate, DateTime start, DateTime end)
        => Round(Hours(start, end) * hourlyRate);

    public decimal ServicesCost(IEnumerable<PricedLine> lines)
        => Round(lines.Sum(line => line.Price * line.Quantity));

    public decimal Total(decimal hourlyRate, DateTime start, DateTime end, IEnumerable<PricedLine> lines)
        => RoomCost(hourlyRate, start, end) + ServicesCost(lines);

    private static decimal Round(decimal value)
        => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
