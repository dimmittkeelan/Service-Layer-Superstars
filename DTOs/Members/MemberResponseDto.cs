namespace LibraryApi.DTOs.Members;

public class MemberResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime MembershipDate { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}
