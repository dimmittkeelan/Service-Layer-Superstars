using LibraryApi.DTOs.Borrow;

namespace LibraryApi.Services;

public interface IBorrowService
{
    Task<BorrowRecordResponseDto> BorrowAsync(BorrowBookRequestDto request, CancellationToken cancellationToken = default);
    Task<BorrowRecordResponseDto> ReturnAsync(ReturnBookRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BorrowRecordResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BorrowRecordResponseDto>> GetByMemberIdAsync(int memberId, CancellationToken cancellationToken = default);
}
