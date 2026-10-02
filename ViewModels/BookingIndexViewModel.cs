using Microsoft.AspNetCore.Mvc.Rendering;

namespace coworking.ViewModels;

public class BookingIndexViewModel
{
    public BookingIndexQuery Query { get; set; } = new();

    public List<BookingRowViewModel> Bookings { get; set; } = [];

    public List<SelectListItem> Statuses { get; set; } = [];

    public List<SelectListItem> Rooms { get; set; } = [];

    public List<SelectListItem> Clients { get; set; } = [];
}
