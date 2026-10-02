using System.ComponentModel.DataAnnotations;

namespace coworking.ViewModels;

public class ClientFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите ФИО.")]
    [StringLength(150, ErrorMessage = "ФИО не длиннее 150 символов.")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Укажите электронную почту.")]
    [EmailAddress(ErrorMessage = "Некорректная электронная почта.")]
    [StringLength(200)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Укажите телефон.")]
    [StringLength(30, ErrorMessage = "Телефон не длиннее 30 символов.")]
    public string Phone { get; set; } = "";

    [StringLength(150, ErrorMessage = "Название организации не длиннее 150 символов.")]
    public string? Company { get; set; }
}
