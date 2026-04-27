using LibraryApi.Data;
using LibraryApi.Dtos;
using LibraryApi.Models;
using LibraryApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LibraryApi.Services
{
    public class BorrowRecordService : IBorrowRecordService
    {
        private readonly IBorrowRecordRepository _borrowRecordRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BorrowRecordService> _logger;

        private const int MaxBorrowsPerMember = 5;
        private const string BorrowRecordsCacheKey = "borrow-records:all";
        private const string BooksCacheKey = "books:all";

        private static string MemberBorrowHistoryCacheKey(Guid memberId) => $"borrow-records:member:{memberId}";
        private static string BookByIdCacheKey(Guid id) => $"books:{id}";

        public BorrowRecordService(
            IBorrowRecordRepository borrowRecordRepository,
            IBookRepository bookRepository,
            IMemberRepository memberRepository,
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<BorrowRecordService> logger)
        {
            _borrowRecordRepository = borrowRecordRepository;
            _bookRepository = bookRepository;
            _memberRepository = memberRepository;
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<BorrowRecordResponse> BorrowBookAsync(BorrowRequest request)
        {
            _logger.LogInformation("Borrow request: book {BookId} for member {MemberId}", request.BookId, request.MemberId);

            var book = await _bookRepository.GetById(request.BookId);
            if (book == null)
                throw new InvalidOperationException("Book not found.");

            var member = await _memberRepository.GetById(request.MemberId);
            if (member == null)
                throw new InvalidOperationException("Member not found.");

            var activeBorrow = await _borrowRecordRepository.GetActiveBorrow(request.BookId, request.MemberId);
            if (activeBorrow != null)
                throw new InvalidOperationException("Member already has an active borrow for this book.");

            var activeBorrowCount = await _borrowRecordRepository.GetActiveBorrowCountByMember(request.MemberId);
            if (activeBorrowCount >= MaxBorrowsPerMember)
                throw new InvalidOperationException($"Member has reached the maximum limit of {MaxBorrowsPerMember} active borrows.");

            if (book.AvailableCopies <= 0)
                throw new InvalidOperationException("No copies of this book are currently available.");

            // Decrement and save atomically — [ConcurrencyCheck] on AvailableCopies detects race conditions
            book.AvailableCopies--;

            var borrowRecord = new BorrowRecord
            {
                Id = Guid.NewGuid(),
                BookId = request.BookId,
                MemberId = request.MemberId,
                BorrowDate = DateTime.UtcNow,
                ReturnDate = null,
                Status = "Borrowed"
            };

            _context.BorrowRecords.Add(borrowRecord);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Book {BookId} borrowed successfully by member {MemberId}", request.BookId, request.MemberId);
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Concurrency conflict borrowing book {BookId} — another request modified availability", request.BookId);
                throw;
            }

            InvalidateCache(request.BookId);
            return MapToResponse(borrowRecord);
        }

        public async Task<BorrowRecordResponse> ReturnBookAsync(ReturnRequest request)
        {
            _logger.LogInformation("Return request for borrow record {BorrowRecordId}", request.BorrowRecordId);

            var borrowRecord = await _borrowRecordRepository.GetById(request.BorrowRecordId);
            if (borrowRecord == null)
                throw new InvalidOperationException("Borrow record not found.");

            if (borrowRecord.Status != "Borrowed")
                throw new InvalidOperationException("This book was not borrowed or has already been returned.");

            var book = await _bookRepository.GetById(borrowRecord.BookId);
            if (book == null)
                throw new InvalidOperationException("Book not found.");

            borrowRecord.Status = "Returned";
            borrowRecord.ReturnDate = DateTime.UtcNow;

            if (book.AvailableCopies < book.TotalCopies)
                book.AvailableCopies++;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Borrow record {BorrowRecordId} returned successfully", request.BorrowRecordId);

            InvalidateCache(borrowRecord.BookId);
            return MapToResponse(borrowRecord);
        }

        public async Task<IEnumerable<BorrowRecordResponse>> GetAllBorrowRecordsAsync()
        {
            _logger.LogInformation("Getting all borrow records");

            if (_cache.TryGetValue(BorrowRecordsCacheKey, out List<BorrowRecordResponse>? cached) && cached != null)
                return cached;

            var records = (await _borrowRecordRepository.GetAll()).Select(MapToResponse).ToList();
            _cache.Set(BorrowRecordsCacheKey, records, TimeSpan.FromMinutes(5));
            return records;
        }

        public async Task<IEnumerable<BorrowRecordResponse>> GetMemberBorrowHistoryAsync(Guid memberId)
        {
            _logger.LogInformation("Getting borrow history for member {MemberId}", memberId);

            var member = await _memberRepository.GetById(memberId);
            if (member == null)
                throw new InvalidOperationException("Member not found.");

            var cacheKey = MemberBorrowHistoryCacheKey(memberId);
            if (_cache.TryGetValue(cacheKey, out List<BorrowRecordResponse>? cached) && cached != null)
                return cached;

            var records = (await _borrowRecordRepository.GetByMemberId(memberId)).Select(MapToResponse).ToList();
            _cache.Set(cacheKey, records, TimeSpan.FromMinutes(5));
            return records;
        }

        private static BorrowRecordResponse MapToResponse(BorrowRecord record) => new()
        {
            Id = record.Id,
            BookId = record.BookId,
            MemberId = record.MemberId,
            BorrowDate = record.BorrowDate,
            ReturnDate = record.ReturnDate,
            Status = record.Status
        };

        private void InvalidateCache(Guid? bookId = null)
        {
            _cache.Remove(BorrowRecordsCacheKey);
            _cache.Remove(BooksCacheKey);
            if (bookId.HasValue)
            {
                _cache.Remove(BookByIdCacheKey(bookId.Value));
                _cache.Remove(MemberBorrowHistoryCacheKey(bookId.Value));
            }
        }
    }
}
