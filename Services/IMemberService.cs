using LibraryApi.DTOs.Members;

namespace LibraryApi.Services;

public interface IMemberService
{
    Task<IReadOnlyCollection<MemberResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MemberResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<MemberResponseDto> CreateAsync(CreateMemberRequestDto request, CancellationToken cancellationToken = default);
    Task<MemberResponseDto> UpdateAsync(int id, UpdateMemberRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
