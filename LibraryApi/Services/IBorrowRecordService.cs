using LibraryApi.Dtos;

namespace LibraryApi.Services
{
    public interface IBorrowRecordService
    {
        Task<BorrowRecordResponse> BorrowBookAsync(BorrowRequest request);
        Task<BorrowRecordResponse> ReturnBookAsync(ReturnRequest request);
        Task<IEnumerable<BorrowRecordResponse>> GetAllBorrowRecordsAsync();
        Task<IEnumerable<BorrowRecordResponse>> GetMemberBorrowHistoryAsync(Guid memberId);
    }
}
