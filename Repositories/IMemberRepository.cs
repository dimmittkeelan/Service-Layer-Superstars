using LibraryApi.Entities;

namespace LibraryApi.Repositories;

public interface IMemberRepository
{
    Task<IReadOnlyCollection<Member>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, int? excludedId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Member member, CancellationToken cancellationToken = default);
    void Update(Member member);
    void Delete(Member member);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
