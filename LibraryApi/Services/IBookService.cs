using LibraryApi.Dtos;

namespace LibraryApi.Services
{
    public interface IBookService
    {
        IEnumerable<BookResponse> GetBooks();
        BookResponse? GetBookById(Guid id);
        BookResponse CreateBook(CreateBookRequest request);
        BookResponse? UpdateBook(Guid id, UpdateBookRequest request);
        BookResponse? DeleteBook(Guid id);
    }
}