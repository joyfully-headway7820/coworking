namespace coworking.ViewModels;

public class BookingServiceInput
{
    public int ServiceId { get; set; }

    public string Name { get; set; } = "";

    public decimal Price { get; set; }

    public bool Selected { get; set; }

    public int Quantity { get; set; } = 1;
}
