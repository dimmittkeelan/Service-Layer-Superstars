using LibraryApi.Data;
using LibraryApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Repositories;

public class BorrowRecordRepository : IBorrowRecordRepository
{
    private readonly LibraryDbContext _context;

    public BorrowRecordRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<BorrowRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.BorrowRecords
            .AsNoTracking()
            .OrderByDescending(br => br.BorrowDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<BorrowRecord>> GetByMemberIdAsync(int memberId, CancellationToken cancellationToken = default)
    {
        return await _context.BorrowRecords
            .AsNoTracking()
            .Where(br => br.MemberId == memberId)
            .OrderByDescending(br => br.BorrowDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<BorrowRecord?> GetActiveBorrowRecordAsync(int memberId, int bookId, CancellationToken cancellationToken = default)
    {
        return await _context.BorrowRecords
            .FirstOrDefaultAsync(
                br => br.MemberId == memberId
                      && br.BookId == bookId
                      && br.Status == BorrowStatus.Borrowed
                      && br.ReturnDate == null,
                cancellationToken);
    }

    public async Task AddAsync(BorrowRecord borrowRecord, CancellationToken cancellationToken = default)
    {
        await _context.BorrowRecords.AddAsync(borrowRecord, cancellationToken);
    }

    public void Update(BorrowRecord borrowRecord)
    {
        _context.BorrowRecords.Update(borrowRecord);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
