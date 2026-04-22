
namespace LibraryApi.Models
{
    /* 
BorrowRecord
    Id
    BookId
    MemberId
    BorrowDate
    ReturnDate
    Status (Borrowed / Returned)
 */
    public class BorrowRecord
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Borrowed";
    }
}