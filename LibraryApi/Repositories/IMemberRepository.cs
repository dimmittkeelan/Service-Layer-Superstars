using LibraryApi.Models;

namespace LibraryApi.Repositories
{
    public interface IMemberRepository
    {
        IEnumerable<Member> GetAll();
        Member? GetById(Guid id);
        void Add(Member member);
        void Update(Member member);
        void Delete(Guid id);
    }
}