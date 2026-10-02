using System.ComponentModel.DataAnnotations;

namespace coworking.ViewModels;

public class BookingFormViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите клиента.")]
    public int ClientId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите помещение.")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Укажите время начала.")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "Укажите время окончания.")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime EndTime { get; set; }

    public List<BookingServiceInput> Services { get; set; } = [];

    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Clients { get; set; } = [];

    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Rooms { get; set; } = [];
}
