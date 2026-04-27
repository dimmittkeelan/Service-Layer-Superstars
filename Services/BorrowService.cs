using LibraryApi.Common.Exceptions;
using LibraryApi.DTOs.Borrow;
using LibraryApi.DTOs.Mappings;
using LibraryApi.Entities;
using LibraryApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class BorrowService : IBorrowService
{
    private readonly IBookRepository _bookRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IBorrowRecordRepository _borrowRecordRepository;
    private readonly IBookCacheService _bookCacheService;

    public BorrowService(
        IBookRepository bookRepository,
        IMemberRepository memberRepository,
        IBorrowRecordRepository borrowRecordRepository,
        IBookCacheService bookCacheService)
    {
        _bookRepository = bookRepository;
        _memberRepository = memberRepository;
        _borrowRecordRepository = borrowRecordRepository;
        _bookCacheService = bookCacheService;
    }

    public async Task<BorrowRecordResponseDto> BorrowAsync(BorrowBookRequestDto request, CancellationToken cancellationToken = default)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
        if (member == null)
        {
            throw new NotFoundException("Member not found.");
        }

        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book == null)
        {
            throw new NotFoundException("Book not found.");
        }

        if (book.AvailableCopies <= 0)
        {
            throw new ConflictException("No available copies for this book.");
        }

        book.AvailableCopies -= 1;

        var record = new BorrowRecord
        {
            BookId = book.Id,
            MemberId = member.Id,
            BorrowDate = DateTime.UtcNow,
            Status = BorrowStatus.Borrowed,
            ReturnDate = null
        };

        _bookRepository.Update(book);
        await _borrowRecordRepository.AddAsync(record, cancellationToken);

        try
        {
            await _bookRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("The last available copy was borrowed by another request. Please retry.");
        }

        _bookCacheService.InvalidateBookCache(book.Id);

        return record.ToBorrowRecordResponse();
    }

    public async Task<BorrowRecordResponseDto> ReturnAsync(ReturnBookRequestDto request, CancellationToken cancellationToken = default)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
        if (member == null)
        {
            throw new NotFoundException("Member not found.");
        }

        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book == null)
        {
            throw new NotFoundException("Book not found.");
        }

        var record = await _borrowRecordRepository.GetActiveBorrowRecordAsync(request.MemberId, request.BookId, cancellationToken);
        if (record == null)
        {
            throw new ConflictException("This member does not have an active borrow for the specified book.");
        }

        if (book.AvailableCopies >= book.TotalCopies)
        {
            throw new ConflictException("Cannot return this book because available copies already match total copies.");
        }

        book.AvailableCopies += 1;
        record.ReturnDate = DateTime.UtcNow;
        record.Status = BorrowStatus.Returned;

        _bookRepository.Update(book);
        _borrowRecordRepository.Update(record);

        try
        {
            await _bookRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("This borrow transaction was updated by another request. Please retry.");
        }

        _bookCacheService.InvalidateBookCache(book.Id);

        return record.ToBorrowRecordResponse();
    }

    public async Task<IReadOnlyCollection<BorrowRecordResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await _borrowRecordRepository.GetAllAsync(cancellationToken);
        return records.Select(r => r.ToBorrowRecordResponse()).ToList();
    }

    public async Task<IReadOnlyCollection<BorrowRecordResponseDto>> GetByMemberIdAsync(int memberId, CancellationToken cancellationToken = default)
    {
        var member = await _memberRepository.GetByIdAsync(memberId, cancellationToken);
        if (member == null)
        {
            throw new NotFoundException("Member not found.");
        }

        var records = await _borrowRecordRepository.GetByMemberIdAsync(memberId, cancellationToken);
        return records.Select(r => r.ToBorrowRecordResponse()).ToList();
    }
}
