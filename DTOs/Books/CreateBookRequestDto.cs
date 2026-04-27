using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs.Books;

public class CreateBookRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Author { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string ISBN { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TotalCopies { get; set; }

    [Range(0, int.MaxValue)]
    public int AvailableCopies { get; set; }
}
