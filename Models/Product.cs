using System.ComponentModel.DataAnnotations;

namespace InternshipProject.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "99999999.99")]
    [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Enter a price with at most two decimal places.")]
    public decimal Price { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }
}
