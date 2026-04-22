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

        public IEnumerable<BorrowRecord> GetAll()
        {
            return _context.BorrowRecords.ToList();
        }

        public BorrowRecord? GetById(Guid id)
        {
            return _context.BorrowRecords.Find(id);
        }

        public void Add(BorrowRecord record)
        {
            _context.BorrowRecords.Add(record);
            _context.SaveChanges();
        }

        public void Update(BorrowRecord record)
        {
            _context.BorrowRecords.Update(record);
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var record = _context.BorrowRecords.Find(id);
            if (record != null)
            {
                _context.BorrowRecords.Remove(record);
                _context.SaveChanges();
            }
        }
    }
}
