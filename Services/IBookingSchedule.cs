using coworking.Models;

namespace coworking.Services;

public interface IBookingSchedule
{
    Task<IReadOnlyList<RuleViolation>> ValidateAsync(BookingDraft draft, CancellationToken cancellationToken);

    Task<IReadOnlyList<RuleViolation>> ValidateLinesAsync(
        IReadOnlyCollection<ServiceLineDraft> lines,
        CancellationToken cancellationToken);

    Task<List<Room>> AvailableRoomsAsync(DateTime start, DateTime end, int capacity, CancellationToken cancellationToken);
}
