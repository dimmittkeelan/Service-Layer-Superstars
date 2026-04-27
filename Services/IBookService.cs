using LibraryApi.DTOs.Books;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<IReadOnlyCollection<BookResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BookResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BookResponseDto> CreateAsync(CreateBookRequestDto request, CancellationToken cancellationToken = default);
    Task<BookResponseDto> UpdateAsync(int id, UpdateBookRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
