using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Dtos
{
    public class BorrowRequest
    {
        [Required(ErrorMessage = "BookId is required.")]
        public Guid BookId { get; set; }

        [Required(ErrorMessage = "MemberId is required.")]
        public Guid MemberId { get; set; }
    }
}
