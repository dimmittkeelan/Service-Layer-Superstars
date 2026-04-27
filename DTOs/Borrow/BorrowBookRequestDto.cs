using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs.Borrow;

public class BorrowBookRequestDto
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int MemberId { get; set; }
}
