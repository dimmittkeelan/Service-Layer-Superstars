using LibraryApi.Dtos;

namespace LibraryApi.Services
{
    public interface IBookService
    {
        Task<PagedResponse<BookResponse>> GetBooksAsync(string? search = null, int page = 1, int pageSize = 10);
        Task<BookResponse?> GetBookByIdAsync(Guid id);
        Task<BookResponse> CreateBookAsync(CreateBookRequest request);
        Task<BookResponse?> UpdateBookAsync(Guid id, UpdateBookRequest request);
        Task<BookResponse?> DeleteBookAsync(Guid id);
    }
}
