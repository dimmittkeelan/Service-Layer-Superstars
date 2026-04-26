using LibraryApi.Models;

namespace LibraryApi.Repositories
{
    public interface IMemberRepository
    {
        Task<List<Member>> GetAll();
        Task<Member?> GetById(Guid id);
        Task<Member> Add(Member member);
        Task Update(Member member);
        Task Delete(Guid id);
    }
}