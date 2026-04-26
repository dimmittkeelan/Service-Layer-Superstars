using LibraryApi.Dtos;
using LibraryApi.Models;
using LibraryApi.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace LibraryApi.Services
{
    public class BorrowRecordService : IBorrowRecordService
    {
        private readonly IBorrowRecordRepository _borrowRecordRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IMemoryCache _cache;
        private const string BorrowRecordsCacheKey = "borrow-records:all";

        private static string MemberBorrowHistoryCacheKey(Guid memberId) => $"borrow-records:member:{memberId}";

        public BorrowRecordService(
            IBorrowRecordRepository borrowRecordRepository,
            IBookRepository bookRepository,
            IMemberRepository memberRepository,
            IMemoryCache cache)
        {
            _borrowRecordRepository = borrowRecordRepository;
            _bookRepository = bookRepository;
            _memberRepository = memberRepository;
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

            // Check if book has available copies
            if (book.AvailableCopies <= 0)
            {
                throw new InvalidOperationException("Book is not available for borrowing.");
            }

            // Check if member already has an active borrow of this book
            var activeBorrow = await _borrowRecordRepository.GetActiveBorrow(request.BookId, request.MemberId);
            if (activeBorrow != null)
            {
                throw new InvalidOperationException("Member already has an active borrow of this book.");
            }

            // Create borrow record
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

            // Decrease available copies
            book.AvailableCopies--;
            await _bookRepository.Update(book);

            InvalidateCache();

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

            // Update borrow record
            borrowRecord.Status = "Returned";
            borrowRecord.ReturnDate = DateTime.UtcNow;
            await _borrowRecordRepository.Update(borrowRecord);

            // Increase available copies
            book.AvailableCopies++;
            await _bookRepository.Update(book);

            InvalidateCache();

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

        private void InvalidateCache()
        {
            _cache.Remove(BorrowRecordsCacheKey);
        }
    }
}
