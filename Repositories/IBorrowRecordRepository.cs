using LibraryApi.Entities;

namespace LibraryApi.Repositories;

public interface IBorrowRecordRepository
{
    Task<IReadOnlyCollection<BorrowRecord>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BorrowRecord>> GetByMemberIdAsync(int memberId, CancellationToken cancellationToken = default);
    Task<BorrowRecord?> GetActiveBorrowRecordAsync(int memberId, int bookId, CancellationToken cancellationToken = default);
    Task AddAsync(BorrowRecord borrowRecord, CancellationToken cancellationToken = default);
    void Update(BorrowRecord borrowRecord);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
