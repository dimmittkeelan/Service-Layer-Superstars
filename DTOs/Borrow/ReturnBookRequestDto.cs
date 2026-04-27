using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs.Borrow;

public class ReturnBookRequestDto
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int MemberId { get; set; }
}
