using LibraryApi.Entities;

namespace LibraryApi.Repositories;

public interface IBookRepository
{
    Task<IReadOnlyCollection<Book>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIsbnAsync(string isbn, int? excludedId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Book book, CancellationToken cancellationToken = default);
    void Update(Book book);
    void Delete(Book book);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
