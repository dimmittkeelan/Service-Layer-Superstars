using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Dtos
{
    public class UpdateMemberRequest
    {
        [MinLength(1, ErrorMessage = "FullName cannot be empty.")]
        public string? FullName { get; set; }

        public string? Email { get; set; }
    }
}
