using LibraryApi.Dtos;
using LibraryApi.Models;
using LibraryApi.Repositories;
using LibraryApi.Data;
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
        private const string BorrowRecordsCacheKey = "borrow-records:all";
        private const string BooksCacheKey = "books:all";

        private static string MemberBorrowHistoryCacheKey(Guid memberId) => $"borrow-records:member:{memberId}";
        private static string BookByIdCacheKey(Guid id) => $"books:{id}";

        public BorrowRecordService(
            IBorrowRecordRepository borrowRecordRepository,
            IBookRepository bookRepository,
            IMemberRepository memberRepository,
            ApplicationDbContext context,
            IMemoryCache cache)
        {
            _borrowRecordRepository = borrowRecordRepository;
            _bookRepository = bookRepository;
            _memberRepository = memberRepository;
            _context = context;
            _cache = cache;
        }

        public async Task<BorrowRecordResponse> BorrowBookAsync(BorrowRequest request)
        {
            // Check if book exists
            var book = await _bookRepository.GetById(request.BookId);
            if (book == null)
            {
                throw new InvalidOperationException("Book not found.");
            }

            // Check if member exists
            var member = await _memberRepository.GetById(request.MemberId);
            if (member == null)
            {
                throw new InvalidOperationException("Member not found.");
            }

            // Check if member already has an active borrow of this book
            var activeBorrow = await _borrowRecordRepository.GetActiveBorrow(request.BookId, request.MemberId);
            if (activeBorrow != null)
            {
                throw new InvalidOperationException("Member already checked-out a copy of this book.");
            }

            // Create borrow record first
            var borrowRecord = new BorrowRecord
            {
                Id = Guid.NewGuid(),
                BookId = request.BookId,
                MemberId = request.MemberId,
                BorrowDate = DateTime.UtcNow,
                ReturnDate = null,
                Status = "Borrowed"
            };

            var createdRecord = await _borrowRecordRepository.Add(borrowRecord);

            // Atomically decrease available copies only if copies > 0 (prevents race condition)
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE Id = {0} AND AvailableCopies > 0",
                request.BookId);

            if (rowsAffected == 0)
            {
                // Book became unavailable, delete the borrow record and throw error
                await _borrowRecordRepository.Delete(createdRecord.Id);
                throw new InvalidOperationException("Book is no longer available for borrowing.");
            }

            InvalidateCache(request.BookId);

            return MapToResponse(createdRecord);
        }

        public async Task<BorrowRecordResponse> ReturnBookAsync(ReturnRequest request)
        {
            // Get the borrow record
            var borrowRecord = await _borrowRecordRepository.GetById(request.BorrowRecordId);
            if (borrowRecord == null)
            {
                throw new InvalidOperationException("Borrow record not found.");
            }

            // Check if it's an active borrow (not already returned)
            if (borrowRecord.Status != "Borrowed")
            {
                throw new InvalidOperationException("This book was not borrowed or has already been returned.");
            }

            // Get the book
            var book = await _bookRepository.GetById(borrowRecord.BookId);
            if (book == null)
            {
                throw new InvalidOperationException("Book not found.");
            }

            // Update borrow record status
            borrowRecord.Status = "Returned";
            borrowRecord.ReturnDate = DateTime.UtcNow;
            await _borrowRecordRepository.Update(borrowRecord);

            // Atomically increase available copies (but not beyond TotalCopies)
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE Id = {0} AND AvailableCopies < TotalCopies",
                borrowRecord.BookId);

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException("Failed to update book availability.");
            }

            InvalidateCache(borrowRecord.BookId);

            return MapToResponse(borrowRecord);
        }

        public async Task<IEnumerable<BorrowRecordResponse>> GetAllBorrowRecordsAsync()
        {
            if (_cache.TryGetValue(BorrowRecordsCacheKey, out List<BorrowRecordResponse>? cachedRecords) && cachedRecords != null)
            {
                return cachedRecords;
            }

            var records = (await _borrowRecordRepository.GetAll()).Select(MapToResponse).ToList();
            _cache.Set(BorrowRecordsCacheKey, records, TimeSpan.FromMinutes(5));
            return records;
        }

        public async Task<IEnumerable<BorrowRecordResponse>> GetMemberBorrowHistoryAsync(Guid memberId)
        {
            // Check if member exists
            var member = await _memberRepository.GetById(memberId);
            if (member == null)
            {
                throw new InvalidOperationException("Member not found.");
            }

            var cacheKey = MemberBorrowHistoryCacheKey(memberId);
            if (_cache.TryGetValue(cacheKey, out List<BorrowRecordResponse>? cachedHistory) && cachedHistory != null)
            {
                return cachedHistory;
            }

            var records = (await _borrowRecordRepository.GetByMemberId(memberId)).Select(MapToResponse).ToList();
            _cache.Set(cacheKey, records, TimeSpan.FromMinutes(5));
            return records;
        }

        private BorrowRecordResponse MapToResponse(BorrowRecord record)
        {
            return new BorrowRecordResponse
            {
                Id = record.Id,
                BookId = record.BookId,
                MemberId = record.MemberId,
                BorrowDate = record.BorrowDate,
                ReturnDate = record.ReturnDate,
                Status = record.Status
            };
        }

        private void InvalidateCache(Guid? bookId = null)
        {
            _cache.Remove(BorrowRecordsCacheKey);
            _cache.Remove(BooksCacheKey);
            if (bookId.HasValue)
            {
                _cache.Remove(BookByIdCacheKey(bookId.Value));
            }
        }
    }
}
