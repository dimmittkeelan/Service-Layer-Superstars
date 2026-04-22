namespace LibraryApi.Models
{
    /* 
    Member:
        Id
        FullName
        Email
        MembershipDate
     */
    public class Member
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime MembershipDate { get; set; }
    }
}