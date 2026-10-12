using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InternshipProject.ViewModels;

public class UserFormViewModel
{
    [Required, StringLength(50)]
    [RegularExpression(@"[a-zA-Z0-9._-]+", ErrorMessage = "Use letters, numbers, dots, underscores or hyphens.")]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(100), Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
    public string? Password { get; set; }

    [Range(1, int.MaxValue), Display(Name = "Role")]
    public int RoleId { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Roles { get; set; } = Array.Empty<SelectListItem>();
}
