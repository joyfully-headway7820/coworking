using Microsoft.AspNetCore.Mvc.Rendering;

namespace coworking.ViewModels;

public class RoomIndexViewModel
{
    public RoomIndexQuery Query { get; set; } = new();

    public List<RoomRowViewModel> Rooms { get; set; } = [];

    public List<SelectListItem> RoomTypes { get; set; } = [];
}
