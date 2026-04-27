using LibraryApi.Common.Exceptions;
using LibraryApi.DTOs.Books;
using LibraryApi.DTOs.Mappings;
using LibraryApi.Entities;
using LibraryApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LibraryApi.Services;

public class BookService : IBookService, IBookCacheService
{
    private const string AllBooksCacheKey = "books:all";
    private readonly IBookRepository _bookRepository;
    private readonly IMemoryCache _cache;

    public BookService(IBookRepository bookRepository, IMemoryCache cache)
    {
        _bookRepository = bookRepository;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<BookResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(AllBooksCacheKey, out IReadOnlyCollection<BookResponseDto>? cachedBooks) && cachedBooks != null)
        {
            return cachedBooks;
        }

        var books = await _bookRepository.GetAllAsync(cancellationToken);
        var mapped = books.Select(b => b.ToBookResponse()).ToList();

        _cache.Set(AllBooksCacheKey, mapped, TimeSpan.FromMinutes(5));
        return mapped;
    }

    public async Task<BookResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var key = GetBookCacheKey(id);
        if (_cache.TryGetValue(key, out BookResponseDto? cachedBook) && cachedBook != null)
        {
            return cachedBook;
        }

        var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
        if (book == null)
        {
            throw new NotFoundException("Book not found.");
        }

        var mapped = book.ToBookResponse();
        _cache.Set(key, mapped, TimeSpan.FromMinutes(5));

        return mapped;
    }

    public async Task<BookResponseDto> CreateAsync(CreateBookRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateCopyCounts(request.TotalCopies, request.AvailableCopies);

        var isbnExists = await _bookRepository.ExistsByIsbnAsync(request.ISBN, null, cancellationToken);
        if (isbnExists)
        {
            throw new ConflictException("A book with this ISBN already exists.");
        }

        var book = new Book
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            ISBN = request.ISBN.Trim(),
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.AvailableCopies
        };

        await _bookRepository.AddAsync(book, cancellationToken);
        await _bookRepository.SaveChangesAsync(cancellationToken);

        InvalidateBookCache(book.Id);

        return book.ToBookResponse();
    }

    public async Task<BookResponseDto> UpdateAsync(int id, UpdateBookRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateCopyCounts(request.TotalCopies, request.AvailableCopies);

        var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
        if (book == null)
        {
            throw new NotFoundException("Book not found.");
        }

        var isbnExists = await _bookRepository.ExistsByIsbnAsync(request.ISBN, id, cancellationToken);
        if (isbnExists)
        {
            throw new ConflictException("A book with this ISBN already exists.");
        }

        book.Title = request.Title.Trim();
        book.Author = request.Author.Trim();
        book.ISBN = request.ISBN.Trim();
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = request.AvailableCopies;

        _bookRepository.Update(book);

        try
        {
            await _bookRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("The book was modified by another request. Please retry.");
        }

        InvalidateBookCache(book.Id);

        return book.ToBookResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
        if (book == null)
        {
            throw new NotFoundException("Book not found.");
        }

        _bookRepository.Delete(book);

        try
        {
            await _bookRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Book cannot be deleted while related borrow records exist.");
        }

        InvalidateBookCache(book.Id);
    }

    public void InvalidateBookCache(int bookId)
    {
        _cache.Remove(AllBooksCacheKey);
        _cache.Remove(GetBookCacheKey(bookId));
    }

    private static string GetBookCacheKey(int bookId)
    {
        return $"books:id:{bookId}";
    }

    private static void ValidateCopyCounts(int totalCopies, int availableCopies)
    {
        if (totalCopies <= 0)
        {
            throw new BusinessValidationException("TotalCopies must be greater than 0.");
        }

        if (availableCopies < 0 || availableCopies > totalCopies)
        {
            throw new BusinessValidationException("AvailableCopies must be between 0 and TotalCopies.");
        }
    }
}
