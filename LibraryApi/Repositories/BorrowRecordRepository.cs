using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Repositories
{
    public class BorrowRecordRepository : IBorrowRecordRepository
    {
        private readonly ApplicationDbContext _context;

        public BorrowRecordRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BorrowRecord>> GetAll()
        {
            return await _context.BorrowRecords.ToListAsync();
        }

        public async Task<BorrowRecord?> GetById(Guid id)
        {
            return await _context.BorrowRecords.FindAsync(id);
        }

        public async Task<List<BorrowRecord>> GetByMemberId(Guid memberId)
        {
            return await _context.BorrowRecords
                .Where(br => br.MemberId == memberId)
                .ToListAsync();
        }

        public async Task<BorrowRecord?> GetActiveBorrow(Guid bookId, Guid memberId)
        {
            return await _context.BorrowRecords
                .FirstOrDefaultAsync(br => br.BookId == bookId && br.MemberId == memberId && br.Status == "Borrowed");
        }

        public async Task<int> GetActiveBorrowCountByMember(Guid memberId)
        {
            return await _context.BorrowRecords
                .CountAsync(br => br.MemberId == memberId && br.Status == "Borrowed");
        }

        public async Task<BorrowRecord> Add(BorrowRecord record)
        {
            _context.BorrowRecords.Add(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task Update(BorrowRecord record)
        {
            _context.BorrowRecords.Update(record);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var record = await _context.BorrowRecords.FindAsync(id);
            if (record != null)
            {
                _context.BorrowRecords.Remove(record);
                await _context.SaveChangesAsync();
            }
        }
    }
}
