using System.ComponentModel.DataAnnotations;

namespace InternshipProject.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(100), Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    // Changing the password invalidates existing login cookies.
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();

    public int RoleId { get; set; } // 3
    public Role Role { get; set; } = null!; 
}
