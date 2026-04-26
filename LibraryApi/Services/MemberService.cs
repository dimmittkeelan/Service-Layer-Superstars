using LibraryApi.Dtos;
using LibraryApi.Models;
using LibraryApi.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace LibraryApi.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IMemoryCache _cache;
        private const string MembersCacheKey = "members:all";

        private static string MemberByIdCacheKey(Guid id) => $"members:{id}";

        public MemberService(IMemberRepository memberRepository, IMemoryCache cache)
        {
            _memberRepository = memberRepository;
            _cache = cache;
        }

        public async Task<IEnumerable<MemberResponse>> GetMembersAsync()
        {
            if (_cache.TryGetValue(MembersCacheKey, out List<MemberResponse>? cachedMembers) && cachedMembers != null)
            {
                return cachedMembers;
            }

            var members = (await _memberRepository.GetAll()).Select(member => new MemberResponse
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                MembershipDate = member.MembershipDate
            }).ToList();

            _cache.Set(MembersCacheKey, members, TimeSpan.FromMinutes(5));
            return members;
        }

        public async Task<MemberResponse?> GetMemberByIdAsync(Guid id)
        {
            var cacheKey = MemberByIdCacheKey(id);
            if (_cache.TryGetValue(cacheKey, out MemberResponse? cachedMember) && cachedMember != null)
            {
                return cachedMember;
            }

            var member = await _memberRepository.GetById(id);
            if (member == null) return null;

            var response = new MemberResponse
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                MembershipDate = member.MembershipDate
            };

            _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
            return response;
        }

        public async Task<MemberResponse> CreateMemberAsync(CreateMemberRequest request)
        {
            var member = new Member
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                MembershipDate = DateTime.UtcNow
            };

            var createdMember = await _memberRepository.Add(member);
            InvalidateCache();

            return new MemberResponse
            {
                Id = createdMember.Id,
                FullName = createdMember.FullName,
                Email = createdMember.Email,
                MembershipDate = createdMember.MembershipDate
            };
        }

        public async Task<MemberResponse?> UpdateMemberAsync(Guid id, UpdateMemberRequest request)
        {
            var member = await _memberRepository.GetById(id);
            if (member == null) return null;

            // Partial update - only update provided fields
            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                member.FullName = request.FullName;
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                member.Email = request.Email;
            }

            await _memberRepository.Update(member);
            InvalidateCache();

            return new MemberResponse
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                MembershipDate = member.MembershipDate
            };
        }

        public async Task DeleteMemberAsync(Guid id)
        {
            await _memberRepository.Delete(id);
            InvalidateCache();
        }

        private void InvalidateCache()
        {
            _cache.Remove(MembersCacheKey);
        }
    }
}
