using System.ComponentModel.DataAnnotations;

namespace InternshipProject.ViewModels;

public class LoginViewModel
{
    [Required, StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
