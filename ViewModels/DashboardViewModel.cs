using Microsoft.AspNetCore.Mvc.Rendering;

namespace coworking.ViewModels;

public class DashboardViewModel
{
    public int RoomCount { get; set; }

    public int ActiveRoomCount { get; set; }

    public int ClientCount { get; set; }

    public int TodayBookingCount { get; set; }

    public int NewBookingCount { get; set; }

    public decimal CompletedTotal { get; set; }

    public string? Status { get; set; }

    public List<SelectListItem> Statuses { get; set; } = [];

    public List<DashboardBookingRowViewModel> Bookings { get; set; } = [];
}
