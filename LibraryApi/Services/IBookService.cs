using LibraryApi.Dtos;
using LibraryApi.Models;

namespace LibraryApi.Services
{
    public interface IBookService
    {
        IEnumerable<Book> GetBooks();
        BookResponse? GetBookById(Guid id);
        BookResponse CreateBook(CreateBookRequest request);
        void UpdateBook(Book book);
        void DeleteBook(Guid id);
    }
}