using LibraryApi.Data;
using LibraryApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly LibraryDbContext _context;

    public MemberRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Member>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .AsNoTracking()
            .OrderBy(m => m.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Members.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludedId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Members.AnyAsync(
            m => m.Email == email && (!excludedId.HasValue || m.Id != excludedId.Value),
            cancellationToken);
    }

    public async Task AddAsync(Member member, CancellationToken cancellationToken = default)
    {
        await _context.Members.AddAsync(member, cancellationToken);
    }

    public void Update(Member member)
    {
        _context.Members.Update(member);
    }

    public void Delete(Member member)
    {
        _context.Members.Remove(member);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
