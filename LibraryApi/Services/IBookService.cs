using LibraryApi.Dtos;
using LibraryApi.Models;

namespace LibraryApi.Services
{
    public interface IBookService
    {
        IEnumerable<Book> GetBooks();
        BookResponse? GetBookById(Guid id);
        BookResponse CreateBook(CreateBookRequest request);
        BookResponse? UpdateBook(Guid id, UpdateBookRequest request);
        BookResponse? DeleteBook(Guid id);
    }
}