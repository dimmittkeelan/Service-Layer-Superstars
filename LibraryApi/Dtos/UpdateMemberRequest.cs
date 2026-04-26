using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Dtos
{
    public class UpdateMemberRequest
    {
        [MinLength(1, ErrorMessage = "FullName cannot be empty.")]
        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string? Email { get; set; }
    }
}
