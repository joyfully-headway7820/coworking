namespace coworking.Services;

public interface IBookingPriceService
{
    decimal Hours(DateTime start, DateTime end);

    decimal RoomCost(decimal hourlyRate, DateTime start, DateTime end);

    decimal ServicesCost(IEnumerable<PricedLine> lines);

    decimal Total(decimal hourlyRate, DateTime start, DateTime end, IEnumerable<PricedLine> lines);
}
