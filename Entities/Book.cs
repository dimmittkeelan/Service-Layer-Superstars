using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Entities;

public class Book
{
    public int Id { get; set; }

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

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
}
