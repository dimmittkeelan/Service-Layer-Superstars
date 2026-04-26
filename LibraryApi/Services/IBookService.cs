using LibraryApi.Dtos;

namespace LibraryApi.Services
{
    public interface IBookService
    {
        Task<IEnumerable<BookResponse>> GetBooksAsync();
        Task<BookResponse?> GetBookByIdAsync(Guid id);
        Task<BookResponse> CreateBookAsync(CreateBookRequest request);
        Task<BookResponse?> UpdateBookAsync(Guid id, UpdateBookRequest request);
        Task<BookResponse?> DeleteBookAsync(Guid id);
    }
}