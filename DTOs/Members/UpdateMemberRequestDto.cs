using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs.Members;

public class UpdateMemberRequestDto
{
    [Required]
    [MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    public DateTime MembershipDate { get; set; }
}
