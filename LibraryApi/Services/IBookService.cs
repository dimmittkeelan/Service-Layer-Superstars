using LibraryApi.Models;

namespace LibraryApi.Services
{
    public interface IBookService
    {
        IEnumerable<Book> GetBooks();
        Book? GetBookById(Guid id);
        void CreateBook(Book book);
        void UpdateBook(Book book);
        void DeleteBook(Guid id);
    }
}