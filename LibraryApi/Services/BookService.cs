using LibraryApi.Dtos;
using LibraryApi.Models;
using LibraryApi.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace LibraryApi.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMemoryCache _cache;
        private const string BooksCacheKey = "books:all";

        private static string BookByIdCacheKey(Guid id) => $"books:{id}";

        public BookService(IBookRepository bookRepository, IMemoryCache cache)
        {
            _bookRepository = bookRepository;
            _cache = cache;
        }

        public IEnumerable<BookResponse> GetBooks()
        {
            if (_cache.TryGetValue(BooksCacheKey, out List<BookResponse>? cachedBooks) && cachedBooks != null)
            {
                return cachedBooks;
            }

            var books = _bookRepository.GetAll().Select(book => new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies
            }).ToList();

            _cache.Set(BooksCacheKey, books, TimeSpan.FromMinutes(5));
            return books;
        }

        public BookResponse? GetBookById(Guid id)
        {
            var cacheKey = BookByIdCacheKey(id);
            if (_cache.TryGetValue(cacheKey, out BookResponse? cachedBook) && cachedBook != null)
            {
                return cachedBook;
            }

            var book = _bookRepository.GetById(id);
            if (book == null) return null;

            var response = new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies
            };

            _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
            return response;
        }

        public BookResponse CreateBook(CreateBookRequest request)
        {
            ValidateBookRules(request.TotalCopies, request.AvailableCopies);

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
            InvalidateBookCache(created.Id);
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
        public BookResponse? UpdateBook(Guid id, UpdateBookRequest request)
        {
            var book = _bookRepository.GetById(id);
            if (book == null) return null;

            var nextTotalCopies = request.TotalCopies ?? book.TotalCopies;
            var nextAvailableCopies = request.AvailableCopies ?? book.AvailableCopies;

            ValidateBookRules(nextTotalCopies, nextAvailableCopies);

            if (request.Title is not null)
            {
                book.Title = request.Title;
            }

            if (request.Author is not null)
            {
                book.Author = request.Author;
            }

            if (request.ISBN is not null)
            {
                book.ISBN = request.ISBN;
            }

            if (request.TotalCopies.HasValue)
            {
                book.TotalCopies = request.TotalCopies.Value;
            }

            if (request.AvailableCopies.HasValue)
            {
                book.AvailableCopies = request.AvailableCopies.Value;
            }

            _bookRepository.Update(book);
            InvalidateBookCache(book.Id);
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
        // todo: error codes for not found
        public BookResponse? DeleteBook(Guid id)
        {
            var book = _bookRepository.GetById(id);
            if (book == null) return null;

            _bookRepository.Delete(id);
            InvalidateBookCache(id);
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

        private static void ValidateBookRules(int totalCopies, int availableCopies)
        {
            if (totalCopies <= 0)
            {
                throw new ArgumentException("TotalCopies must be greater than 0.");
            }

            if (availableCopies < 0)
            {
                throw new ArgumentException("AvailableCopies must be greater than or equal to 0.");
            }

            if (availableCopies > totalCopies)
            {
                throw new ArgumentException("AvailableCopies must not exceed TotalCopies.");
            }
        }

        private void InvalidateBookCache(Guid id)
        {
            _cache.Remove(BooksCacheKey);
            _cache.Remove(BookByIdCacheKey(id));
        }
    }
}