using System.ComponentModel.DataAnnotations;

namespace coworking.Dtos;

public class BookingServiceRequest
{
    [Range(1, int.MaxValue)]
    public int ServiceId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }
}
