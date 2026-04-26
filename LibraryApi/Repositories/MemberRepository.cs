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

        public async Task<List<Member>> GetAll()
        {
            return await _context.Members.ToListAsync();
        }

        public async Task<Member?> GetById(Guid id)
        {
            return await _context.Members.FindAsync(id);
        }

        public async Task<Member> Add(Member member)
        {
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task Update(Member member)
        {
            _context.Members.Update(member);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
            }
        }
    }
}