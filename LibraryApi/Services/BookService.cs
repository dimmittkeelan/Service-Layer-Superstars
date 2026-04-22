using LibraryApi.Dtos;
using LibraryApi.Models;
using LibraryApi.Repositories;

namespace LibraryApi.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public IEnumerable<Book> GetBooks()
        {
            return _bookRepository.GetAll();
        }

        public BookResponse? GetBookById(Guid id)
        {
            var book = _bookRepository.GetById(id);
            if (book == null) return null;

            return new BookResponse {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN
            };
        }

        public BookResponse CreateBook(CreateBookRequest request)
        {
            var book = new Book {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Author = request.Author,
                ISBN = request.ISBN
            };
            var created = _bookRepository.Add(book);
            return new BookResponse { 
                Id = created.Id,
                Title = created.Title,
                Author = created.Author,
                ISBN = created.ISBN
             };
        }

        public void UpdateBook(Book book)
        {
            _bookRepository.Update(book);
        }

        public void DeleteBook(Guid id)
        {
            _bookRepository.Delete(id);
        }
    }
}