namespace coworking.ViewModels;

public class BookingServiceRowViewModel
{
    public int ServiceId { get; set; }

    public string Name { get; set; } = "";

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}
