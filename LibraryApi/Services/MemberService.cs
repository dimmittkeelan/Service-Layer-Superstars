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
        private readonly ILogger<MemberService> _logger;
        private const string MembersCacheKey = "members:all";

        private static string MemberByIdCacheKey(Guid id) => $"members:{id}";

        public MemberService(IMemberRepository memberRepository, IMemoryCache cache, ILogger<MemberService> logger)
        {
            _memberRepository = memberRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IEnumerable<MemberResponse>> GetMembersAsync()
        {
            _logger.LogInformation("Getting all members");

            if (_cache.TryGetValue(MembersCacheKey, out List<MemberResponse>? cached) && cached != null)
                return cached;

            var members = (await _memberRepository.GetAll()).Select(m => new MemberResponse
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                MembershipDate = m.MembershipDate
            }).ToList();

            _cache.Set(MembersCacheKey, members, TimeSpan.FromMinutes(5));
            return members;
        }

        public async Task<MemberResponse?> GetMemberByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting member by ID {Id}", id);

            var cacheKey = MemberByIdCacheKey(id);
            if (_cache.TryGetValue(cacheKey, out MemberResponse? cached) && cached != null)
                return cached;

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
            _logger.LogInformation("Creating member: {FullName}, {Email}", request.FullName, request.Email);

            var member = new Member
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                MembershipDate = DateTime.UtcNow
            };

            var created = await _memberRepository.Add(member);
            InvalidateCache();

            return new MemberResponse
            {
                Id = created.Id,
                FullName = created.FullName,
                Email = created.Email,
                MembershipDate = created.MembershipDate
            };
        }

        public async Task<MemberResponse?> UpdateMemberAsync(Guid id, UpdateMemberRequest request)
        {
            _logger.LogInformation("Updating member {Id}", id);

            var member = await _memberRepository.GetById(id);
            if (member == null) return null;

            if (!string.IsNullOrWhiteSpace(request.FullName)) member.FullName = request.FullName;
            if (!string.IsNullOrWhiteSpace(request.Email)) member.Email = request.Email;

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
            _logger.LogInformation("Deleting member {Id}", id);
            await _memberRepository.Delete(id);
            InvalidateCache();
        }

        private void InvalidateCache()
        {
            _cache.Remove(MembersCacheKey);
        }
    }
}
