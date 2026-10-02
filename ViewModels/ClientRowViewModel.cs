namespace coworking.ViewModels;

public class ClientRowViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Phone { get; set; } = "";

    public string? Company { get; set; }

    public int BookingCount { get; set; }
}
