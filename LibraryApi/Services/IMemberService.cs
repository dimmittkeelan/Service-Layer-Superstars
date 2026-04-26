using LibraryApi.Dtos;

namespace LibraryApi.Services
{
    public interface IMemberService
    {
        Task<IEnumerable<MemberResponse>> GetMembersAsync();
        Task<MemberResponse?> GetMemberByIdAsync(Guid id);
        Task<MemberResponse> CreateMemberAsync(CreateMemberRequest request);
        Task<MemberResponse?> UpdateMemberAsync(Guid id, UpdateMemberRequest request);
        Task DeleteMemberAsync(Guid id);
    }
}
