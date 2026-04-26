using LibraryApi.Models;
namespace LibraryApi.Repositories
{
    public interface IBorrowRecordRepository
    {
        Task<List<BorrowRecord>> GetAll();
        Task<BorrowRecord?> GetById(Guid id);
        Task<List<BorrowRecord>> GetByMemberId(Guid memberId);
        Task<BorrowRecord?> GetActiveBorrow(Guid bookId, Guid memberId);
        Task<BorrowRecord> Add(BorrowRecord record);
        Task Update(BorrowRecord record);
        Task Delete(Guid id);
    }
}