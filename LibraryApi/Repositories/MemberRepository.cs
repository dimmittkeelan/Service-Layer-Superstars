using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly ApplicationDbContext _context;

        public MemberRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Member> GetAll()
        {
            return _context.Members.ToList();
        }

        public Member? GetById(Guid id)
        {
            return _context.Members.Find(id);
        }

        public void Add(Member member)
        {
            _context.Members.Add(member);
            _context.SaveChanges();
        }

        public void Update(Member member)
        {
            _context.Members.Update(member);
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var member = _context.Members.Find(id);
            if (member != null)
            {
                _context.Members.Remove(member);
                _context.SaveChanges();
            }
        }
    }
}