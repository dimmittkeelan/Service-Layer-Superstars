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
        private readonly ILogger<BookService> _logger;
        private const string BooksCacheKey = "books:all";

        private static string BookByIdCacheKey(Guid id) => $"books:{id}";

        public BookService(IBookRepository bookRepository, IMemoryCache cache, ILogger<BookService> logger)
        {
            _bookRepository = bookRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PagedResponse<BookResponse>> GetBooksAsync(string? search = null, int page = 1, int pageSize = 10)
        {
            _logger.LogInformation("Getting books: search={Search}, page={Page}, pageSize={PageSize}", search, page, pageSize);

            List<BookResponse> allBooks;
            if (!_cache.TryGetValue(BooksCacheKey, out List<BookResponse>? cached) || cached == null)
            {
                allBooks = (await _bookRepository.GetAll()).Select(MapToResponse).ToList();
                _cache.Set(BooksCacheKey, allBooks, TimeSpan.FromMinutes(5));
            }
            else
            {
                allBooks = cached;
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLowerInvariant();
                allBooks = allBooks
                    .Where(b => b.Title.ToLowerInvariant().Contains(term)
                             || b.Author.ToLowerInvariant().Contains(term)
                             || b.ISBN.ToLowerInvariant().Contains(term))
                    .ToList();
            }

            var totalCount = allBooks.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var items = allBooks.Skip((page - 1) * pageSize).Take(pageSize);

            return new PagedResponse<BookResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = Math.Max(1, totalPages)
            };
        }

        public async Task<BookResponse?> GetBookByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting book by ID {Id}", id);

            var cacheKey = BookByIdCacheKey(id);
            if (_cache.TryGetValue(cacheKey, out BookResponse? cached) && cached != null)
                return cached;

            var book = await _bookRepository.GetById(id);
            if (book == null) return null;

            var response = MapToResponse(book);
            _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
            return response;
        }

        public async Task<BookResponse> CreateBookAsync(CreateBookRequest request)
        {
            _logger.LogInformation("Creating book: {Title} by {Author}", request.Title, request.Author);
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

            var created = await _bookRepository.Add(book);
            InvalidateBookCache(created.Id);
            return MapToResponse(created);
        }

        public async Task<BookResponse?> UpdateBookAsync(Guid id, UpdateBookRequest request)
        {
            _logger.LogInformation("Updating book {Id}", id);

            var book = await _bookRepository.GetById(id);
            if (book == null) return null;

            var nextTotal = request.TotalCopies ?? book.TotalCopies;
            var nextAvailable = request.AvailableCopies ?? book.AvailableCopies;
            ValidateBookRules(nextTotal, nextAvailable);

            if (request.Title is not null) book.Title = request.Title;
            if (request.Author is not null) book.Author = request.Author;
            if (request.ISBN is not null) book.ISBN = request.ISBN;
            if (request.TotalCopies.HasValue) book.TotalCopies = request.TotalCopies.Value;
            if (request.AvailableCopies.HasValue) book.AvailableCopies = request.AvailableCopies.Value;

            await _bookRepository.Update(book);
            InvalidateBookCache(book.Id);
            return MapToResponse(book);
        }

        public async Task<BookResponse?> DeleteBookAsync(Guid id)
        {
            _logger.LogInformation("Deleting book {Id}", id);

            var book = await _bookRepository.GetById(id);
            if (book == null) return null;

            await _bookRepository.Delete(id);
            InvalidateBookCache(id);
            return MapToResponse(book);
        }

        private static void ValidateBookRules(int totalCopies, int availableCopies)
        {
            if (totalCopies <= 0)
                throw new ArgumentException("TotalCopies must be greater than 0.");
            if (availableCopies < 0)
                throw new ArgumentException("AvailableCopies must be greater than or equal to 0.");
            if (availableCopies > totalCopies)
                throw new ArgumentException("AvailableCopies must not exceed TotalCopies.");
        }

        private static BookResponse MapToResponse(Book book) => new()
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies
        };

        private void InvalidateBookCache(Guid id)
        {
            _cache.Remove(BooksCacheKey);
            _cache.Remove(BookByIdCacheKey(id));
        }
    }
}
