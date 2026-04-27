using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models
{
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        [ConcurrencyCheck]
        public int AvailableCopies { get; set; }
    }
}
