using System.ComponentModel.DataAnnotations;

namespace coworking.ViewModels;

public class RoomFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите название.")]
    [StringLength(100, ErrorMessage = "Название не длиннее 100 символов.")]
    public string Name { get; set; } = "";

    [Range(1, 500, ErrorMessage = "Вместимость должна быть от 1 до 500.")]
    public int Capacity { get; set; }

    [Range(typeof(decimal), "0", "1000000", ErrorMessage = "Укажите цену за час от 0 до 1 000 000.")]
    public decimal HourlyRate { get; set; }

    [Required(ErrorMessage = "Укажите тип помещения.")]
    [StringLength(50, ErrorMessage = "Тип не длиннее 50 символов.")]
    public string RoomType { get; set; } = "";

    public bool IsActive { get; set; } = true;
}
