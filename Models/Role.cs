using System.ComponentModel.DataAnnotations;

namespace InternshipProject.Models;

public class Role
{
    public const string Write = "Write";
    public const string Read = "Read";

    public int Id { get; set; }

    [Required, StringLength(20)]
    public string Name { get; set; } = string.Empty;

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
