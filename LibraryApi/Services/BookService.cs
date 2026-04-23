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

            return new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies
            };
        }

        public BookResponse CreateBook(CreateBookRequest request)
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Author = request.Author,
                ISBN = request.ISBN,
                TotalCopies = request.TotalCopies,
                AvailableCopies = request.AvailableCopies
            };
            var created = _bookRepository.Add(book);
            return new BookResponse
            {
                Id = created.Id,
                Title = created.Title,
                Author = created.Author,
                ISBN = created.ISBN,
                AvailableCopies = created.AvailableCopies,
                TotalCopies = created.TotalCopies
            };
        }
        // TODO: only update the fields that are provided in the request, not all of them
        public BookResponse? UpdateBook(Guid id, UpdateBookRequest request)
        {
            var book = _bookRepository.GetById(id);
            if (book == null) return null;

            book.Title = request.Title;
            book.Author = request.Author;
            book.ISBN = request.ISBN;
            book.TotalCopies = request.TotalCopies;
            book.AvailableCopies = request.AvailableCopies;

            _bookRepository.Update(book);
            return new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN
            };
        }
        // todo: error codes for not found
        public BookResponse? DeleteBook(Guid id)
        {
            var book = _bookRepository.GetById(id);
            if (book == null) return null;

            _bookRepository.Delete(id);
            return new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                AvailableCopies = book.AvailableCopies,
                TotalCopies = book.TotalCopies
            };
        }
    }
}