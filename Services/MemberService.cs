using LibraryApi.Common.Exceptions;
using LibraryApi.DTOs.Mappings;
using LibraryApi.DTOs.Members;
using LibraryApi.Entities;
using LibraryApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<IReadOnlyCollection<MemberResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var members = await _memberRepository.GetAllAsync(cancellationToken);
        return members.Select(m => m.ToMemberResponse()).ToList();
    }

    public async Task<MemberResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var member = await _memberRepository.GetByIdAsync(id, cancellationToken);
        if (member == null)
        {
            throw new NotFoundException("Member not found.");
        }

        return member.ToMemberResponse();
    }

    public async Task<MemberResponseDto> CreateAsync(CreateMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        var emailExists = await _memberRepository.ExistsByEmailAsync(request.Email.Trim(), null, cancellationToken);
        if (emailExists)
        {
            throw new ConflictException("A member with this email already exists.");
        }

        var member = new Member
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            MembershipDate = request.MembershipDate ?? DateTime.UtcNow
        };

        await _memberRepository.AddAsync(member, cancellationToken);
        await _memberRepository.SaveChangesAsync(cancellationToken);

        return member.ToMemberResponse();
    }

    public async Task<MemberResponseDto> UpdateAsync(int id, UpdateMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        var member = await _memberRepository.GetByIdAsync(id, cancellationToken);
        if (member == null)
        {
            throw new NotFoundException("Member not found.");
        }

        var emailExists = await _memberRepository.ExistsByEmailAsync(request.Email.Trim(), id, cancellationToken);
        if (emailExists)
        {
            throw new ConflictException("A member with this email already exists.");
        }

        member.FullName = request.FullName.Trim();
        member.Email = request.Email.Trim();
        member.MembershipDate = request.MembershipDate;

        _memberRepository.Update(member);

        try
        {
            await _memberRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("The member was modified by another request. Please retry.");
        }

        return member.ToMemberResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var member = await _memberRepository.GetByIdAsync(id, cancellationToken);
        if (member == null)
        {
            throw new NotFoundException("Member not found.");
        }

        _memberRepository.Delete(member);

        try
        {
            await _memberRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Member cannot be deleted while related borrow records exist.");
        }
    }
}
