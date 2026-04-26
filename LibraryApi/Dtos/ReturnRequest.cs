using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Dtos
{
    public class ReturnRequest
    {
        [Required(ErrorMessage = "BorrowRecordId is required.")]
        public Guid BorrowRecordId { get; set; }
    }
}
