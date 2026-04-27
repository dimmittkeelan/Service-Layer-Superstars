using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Entities;

public class BorrowRecord
{
    public int Id { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required]
    public int MemberId { get; set; }

    public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

    public DateTime? ReturnDate { get; set; }

    public BorrowStatus Status { get; set; } = BorrowStatus.Borrowed;

    public Book? Book { get; set; }

    public Member? Member { get; set; }
}
