using LibraryApi.Models;
namespace LibraryApi.Repositories
{
    public interface IBorrowRecordRepository
    {
        IEnumerable<BorrowRecord> GetAll();
        BorrowRecord? GetById(Guid id);
        void Add(BorrowRecord record);
        void Update(BorrowRecord record);
        void Delete(Guid id);
    }
}