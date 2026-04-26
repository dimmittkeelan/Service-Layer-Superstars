using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Dtos
{
    public class CreateMemberRequest
    {
        [Required(ErrorMessage = "FullName is required.")]
        [MinLength(1, ErrorMessage = "FullName cannot be empty.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }
}
