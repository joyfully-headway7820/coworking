using System.ComponentModel.DataAnnotations;

namespace coworking.Dtos;

public class UpdateBookingStatusRequest
{
    [Required]
    public string Status { get; set; } = "";
}
