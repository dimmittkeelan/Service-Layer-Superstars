using LibraryApi.Dtos;
using LibraryApi.Models;
using LibraryApi.Repositories;
using LibraryApi.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LibraryApi.Tests.Services
{
    public class MemberServiceTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────────

        private static IMemoryCache CreateCache() =>
            new MemoryCache(new MemoryCacheOptions());

        private static Member MakeMember(
            string fullName = "Jane Doe",
            string email = "jane@example.com") => new Member
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                MembershipDate = DateTime.UtcNow
            };

        private static MemberService CreateService(
            Mock<IMemberRepository> repoMock,
            IMemoryCache? cache = null) =>
            new MemberService(
                repoMock.Object,
                cache ?? CreateCache(),
                NullLogger<MemberService>.Instance);

        // ── GetMembersAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetMembersAsync_ReturnsAllMembers()
        {
            // Arrange
            var members = new List<Member>
            {
                MakeMember("Alice", "alice@example.com"),
                MakeMember("Bob", "bob@example.com"),
                MakeMember("Carol", "carol@example.com")
            };

            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.GetAll()).ReturnsAsync(members);

            var service = CreateService(repoMock);

            // Act
            var result = await service.GetMembersAsync();

            // Assert
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetMembersAsync_ReturnsEmptyList_WhenNoMembers()
        {
            // Arrange
            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<Member>());

            var service = CreateService(repoMock);

            // Act
            var result = await service.GetMembersAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMembersAsync_ServesFromCache_OnSecondCall()
        {
            // Arrange
            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<Member> { MakeMember() });

            var service = CreateService(repoMock);

            // Act — call twice
            await service.GetMembersAsync();
            await service.GetMembersAsync();

            // Assert — repository hit only once (cache on second call)
            repoMock.Verify(r => r.GetAll(), Times.Once);
        }

        // ── GetMemberByIdAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetMemberByIdAsync_ReturnsMember_WhenExists()
        {
            // Arrange
            var member = MakeMember("Alice", "alice@example.com");
            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.GetById(member.Id)).ReturnsAsync(member);

            var service = CreateService(repoMock);

            // Act
            var result = await service.GetMemberByIdAsync(member.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(member.Id, result.Id);
            Assert.Equal("Alice", result.FullName);
            Assert.Equal("alice@example.com", result.Email);
        }

        [Fact]
        public async Task GetMemberByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Member?)null);

            var service = CreateService(repoMock);

            // Act
            var result = await service.GetMemberByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMemberByIdAsync_CachesResult_AfterFirstFetch()
        {
            // Arrange
            var member = MakeMember();
            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.GetById(member.Id)).ReturnsAsync(member);

            var service = CreateService(repoMock);

            // Act — call twice for the same member
            await service.GetMemberByIdAsync(member.Id);
            await service.GetMemberByIdAsync(member.Id);

            // Assert — repo only queried once
            repoMock.Verify(r => r.GetById(member.Id), Times.Once);
        }

        // ── CreateMemberAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task CreateMemberAsync_ReturnsCreatedMember_WithCorrectFields()
        {
            // Arrange
            var request = new CreateMemberRequest
            {
                FullName = "John Smith",
                Email = "john.smith@example.com"
            };

            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.Add(It.IsAny<Member>()))
                    .ReturnsAsync((Member m) => m);

            var service = CreateService(repoMock);

            // Act
            var result = await service.CreateMemberAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Smith", result.FullName);
            Assert.Equal("john.smith@example.com", result.Email);
            Assert.NotEqual(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task CreateMemberAsync_SetsCurrentMembershipDate()
        {
            // Arrange
            var request = new CreateMemberRequest
            {
                FullName = "Jane Doe",
                Email = "jane@example.com"
            };

            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.Add(It.IsAny<Member>()))
                    .ReturnsAsync((Member m) => m);

            var service = CreateService(repoMock);
            var before = DateTime.UtcNow;

            // Act
            var result = await service.CreateMemberAsync(request);

            // Assert — membership date should be set to roughly now
            Assert.True(result.MembershipDate >= before);
            Assert.True(result.MembershipDate <= DateTime.UtcNow);
        }

        [Fact]
        public async Task CreateMemberAsync_CallsRepository_Add_Once()
        {
            // Arrange
            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.Add(It.IsAny<Member>()))
                    .ReturnsAsync((Member m) => m);

            var service = CreateService(repoMock);

            // Act
            await service.CreateMemberAsync(new CreateMemberRequest
            {
                FullName = "Test",
                Email = "test@test.com"
            });

            // Assert
            repoMock.Verify(r => r.Add(It.IsAny<Member>()), Times.Once);
        }

        // ── UpdateMemberAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateMemberAsync_UpdatesFullName_WhenProvided()
        {
            // Arrange
            var existing = MakeMember("Old Name", "old@example.com");
            var request = new UpdateMemberRequest { FullName = "New Name" };

            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.GetById(existing.Id)).ReturnsAsync(existing);
            repoMock.Setup(r => r.Update(It.IsAny<Member>())).Returns(Task.CompletedTask);

            var service = CreateService(repoMock);

            // Act
            var result = await service.UpdateMemberAsync(existing.Id, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.FullName);
            Assert.Equal("old@example.com", result.Email); // unchanged
        }

        [Fact]
        public async Task UpdateMemberAsync_UpdatesEmail_WhenProvided()
        {
            // Arrange
            var existing = MakeMember("Jane Doe", "old@example.com");
            var request = new UpdateMemberRequest { Email = "new@example.com" };

            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.GetById(existing.Id)).ReturnsAsync(existing);
            repoMock.Setup(r => r.Update(It.IsAny<Member>())).Returns(Task.CompletedTask);

            var service = CreateService(repoMock);

            // Act
            var result = await service.UpdateMemberAsync(existing.Id, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new@example.com", result.Email);
            Assert.Equal("Jane Doe", result.FullName); // unchanged
        }

        [Fact]
        public async Task UpdateMemberAsync_ReturnsNull_WhenMemberNotFound()
        {
            // Arrange
            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Member?)null);

            var service = CreateService(repoMock);

            // Act
            var result = await service.UpdateMemberAsync(Guid.NewGuid(), new UpdateMemberRequest());

            // Assert
            Assert.Null(result);
        }

        // ── DeleteMemberAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteMemberAsync_CallsRepository_Delete_Once()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.Delete(memberId)).Returns(Task.CompletedTask);

            var service = CreateService(repoMock);

            // Act
            await service.DeleteMemberAsync(memberId);

            // Assert
            repoMock.Verify(r => r.Delete(memberId), Times.Once);
        }

        [Fact]
        public async Task DeleteMemberAsync_InvalidatesCache()
        {
            // Arrange
            var member = MakeMember();
            var repoMock = new Mock<IMemberRepository>();
            repoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<Member> { member });
            repoMock.Setup(r => r.Delete(It.IsAny<Guid>())).Returns(Task.CompletedTask);

            var cache = CreateCache();
            var service = CreateService(repoMock, cache);

            // Warm up cache
            await service.GetMembersAsync();

            // Act — delete should bust the cache
            await service.DeleteMemberAsync(member.Id);

            // Next GetAll call should hit repo again
            await service.GetMembersAsync();

            // Assert — repo.GetAll called twice: before delete and after delete
            repoMock.Verify(r => r.GetAll(), Times.Exactly(2));
        }
    }
}
