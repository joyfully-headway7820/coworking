namespace coworking.ViewModels;

public class RoomIndexQuery
{
    public string? RoomType { get; set; }

    public int? MinCapacity { get; set; }

    public decimal? MaxPrice { get; set; }

    public bool ActiveOnly { get; set; }
}
