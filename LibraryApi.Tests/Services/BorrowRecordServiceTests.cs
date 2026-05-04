using LibraryApi.Data;
using LibraryApi.Dtos;
using LibraryApi.Models;
using LibraryApi.Repositories;
using LibraryApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LibraryApi.Tests.Services
{
    public class BorrowRecordServiceTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────────

        private static IMemoryCache CreateCache() =>
            new MemoryCache(new MemoryCacheOptions());

        private static ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private static Book MakeBook(int availableCopies = 3, int totalCopies = 3) => new Book
        {
            Id = Guid.NewGuid(),
            Title = "Test Book",
            Author = "Author",
            ISBN = Guid.NewGuid().ToString(),
            TotalCopies = totalCopies,
            AvailableCopies = availableCopies
        };

        private static Member MakeMember() => new Member
        {
            Id = Guid.NewGuid(),
            FullName = "Test Member",
            Email = "test@example.com",
            MembershipDate = DateTime.UtcNow
        };

        private BorrowRecordService CreateService(
            ApplicationDbContext context,
            Mock<IBorrowRecordRepository> borrowRepoMock,
            Mock<IBookRepository> bookRepoMock,
            Mock<IMemberRepository> memberRepoMock) =>
            new BorrowRecordService(
                borrowRepoMock.Object,
                bookRepoMock.Object,
                memberRepoMock.Object,
                context,
                CreateCache(),
                NullLogger<BorrowRecordService>.Instance);

        // ── BorrowBookAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task BorrowBookAsync_SucceedsAndDecrementsAvailableCopies()
        {
            // Arrange
            var context = CreateDbContext();
            var book = MakeBook(availableCopies: 3);
            var member = MakeMember();

            context.Books.Add(book);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            var bookRepoMock = new Mock<IBookRepository>();
            bookRepoMock.Setup(r => r.GetById(book.Id)).ReturnsAsync(book);

            var memberRepoMock = new Mock<IMemberRepository>();
            memberRepoMock.Setup(r => r.GetById(member.Id)).ReturnsAsync(member);

            var borrowRepoMock = new Mock<IBorrowRecordRepository>();
            borrowRepoMock.Setup(r => r.GetActiveBorrow(book.Id, member.Id))
                          .ReturnsAsync((BorrowRecord?)null);
            borrowRepoMock.Setup(r => r.GetActiveBorrowCountByMember(member.Id))
                          .ReturnsAsync(0);

            var service = CreateService(context, borrowRepoMock, bookRepoMock, memberRepoMock);

            // Act
            var result = await service.BorrowBookAsync(new BorrowRequest
            {
                BookId = book.Id,
                MemberId = member.Id
            });

            // Assert
            Assert.Equal("Borrowed", result.Status);
            Assert.Equal(book.Id, result.BookId);
            Assert.Equal(member.Id, result.MemberId);
            Assert.Null(result.ReturnDate);

            // AvailableCopies should have dropped by 1
            Assert.Equal(2, book.AvailableCopies);
        }

        [Fact]
        public async Task BorrowBookAsync_Throws_WhenBookNotFound()
        {
            // Arrange
            var context = CreateDbContext();
            var bookRepoMock = new Mock<IBookRepository>();
            bookRepoMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Book?)null);

            var memberRepoMock = new Mock<IMemberRepository>();
            var borrowRepoMock = new Mock<IBorrowRecordRepository>();

            var service = CreateService(context, borrowRepoMock, bookRepoMock, memberRepoMock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.BorrowBookAsync(new BorrowRequest
                {
                    BookId = Guid.NewGuid(),
                    MemberId = Guid.NewGuid()
                }));
        }

        [Fact]
        public async Task BorrowBookAsync_Throws_WhenMemberNotFound()
        {
            // Arrange
            var context = CreateDbContext();
            var book = MakeBook();

            var bookRepoMock = new Mock<IBookRepository>();
            bookRepoMock.Setup(r => r.GetById(book.Id)).ReturnsAsync(book);

            var memberRepoMock = new Mock<IMemberRepository>();
            memberRepoMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Member?)null);

            var borrowRepoMock = new Mock<IBorrowRecordRepository>();

            var service = CreateService(context, borrowRepoMock, bookRepoMock, memberRepoMock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.BorrowBookAsync(new BorrowRequest
                {
                    BookId = book.Id,
                    MemberId = Guid.NewGuid()
                }));
        }

        [Fact]
        public async Task BorrowBookAsync_Throws_WhenNoAvailableCopies()
        {
            // Arrange — book with 0 available copies
            var context = CreateDbContext();
            var book = MakeBook(availableCopies: 0, totalCopies: 3);
            var member = MakeMember();

            context.Books.Add(book);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            var bookRepoMock = new Mock<IBookRepository>();
            bookRepoMock.Setup(r => r.GetById(book.Id)).ReturnsAsync(book);

            var memberRepoMock = new Mock<IMemberRepository>();
            memberRepoMock.Setup(r => r.GetById(member.Id)).ReturnsAsync(member);

            var borrowRepoMock = new Mock<IBorrowRecordRepository>();
            borrowRepoMock.Setup(r => r.GetActiveBorrow(book.Id, member.Id))
                          .ReturnsAsync((BorrowRecord?)null);
            borrowRepoMock.Setup(r => r.GetActiveBorrowCountByMember(member.Id))
                          .ReturnsAsync(0);

            var service = CreateService(context, borrowRepoMock, bookRepoMock, memberRepoMock);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.BorrowBookAsync(new BorrowRequest
                {
                    BookId = book.Id,
                    MemberId = member.Id
                }));

            Assert.Contains("available", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task BorrowBookAsync_Throws_WhenMemberAlreadyHasActiveBorrow()
        {
            // Arrange
            var context = CreateDbContext();
            var book = MakeBook(availableCopies: 2);
            var member = MakeMember();

            context.Books.Add(book);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            var existingBorrow = new BorrowRecord
            {
                Id = Guid.NewGuid(),
                BookId = book.Id,
                MemberId = member.Id,
                BorrowDate = DateTime.UtcNow,
                Status = "Borrowed"
            };

            var bookRepoMock = new Mock<IBookRepository>();
            bookRepoMock.Setup(r => r.GetById(book.Id)).ReturnsAsync(book);

            var memberRepoMock = new Mock<IMemberRepository>();
            memberRepoMock.Setup(r => r.GetById(member.Id)).ReturnsAsync(member);

            var borrowRepoMock = new Mock<IBorrowRecordRepository>();
            // Member already has an active borrow for this book
            borrowRepoMock.Setup(r => r.GetActiveBorrow(book.Id, member.Id))
                          .ReturnsAsync(existingBorrow);
            borrowRepoMock.Setup(r => r.GetActiveBorrowCountByMember(member.Id))
                          .ReturnsAsync(1);

            var service = CreateService(context, borrowRepoMock, bookRepoMock, memberRepoMock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.BorrowBookAsync(new BorrowRequest
                {
                    BookId = book.Id,
                    MemberId = member.Id
                }));
        }

        [Fact]
        public async Task BorrowBookAsync_Throws_WhenMemberExceedsMaxBorrowLimit()
        {
            // Arrange — member already has 5 active borrows (the max)
            var context = CreateDbContext();
            var book = MakeBook(availableCopies: 2);
            var member = MakeMember();

            context.Books.Add(book);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            var bookRepoMock = new Mock<IBookRepository>();
            bookRepoMock.Setup(r => r.GetById(book.Id)).ReturnsAsync(book);

            var memberRepoMock = new Mock<IMemberRepository>();
            memberRepoMock.Setup(r => r.GetById(member.Id)).ReturnsAsync(member);

            var borrowRepoMock = new Mock<IBorrowRecordRepository>();
            borrowRepoMock.Setup(r => r.GetActiveBorrow(book.Id, member.Id))
                          .ReturnsAsync((BorrowRecord?)null);
            borrowRepoMock.Setup(r => r.GetActiveBorrowCountByMember(member.Id))
                          .ReturnsAsync(5); // ← at the limit

            var service = CreateService(context, borrowRepoMock, bookRepoMock, memberRepoMock);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.BorrowBookAsync(new BorrowRequest
                {
                    BookId = book.Id,
                    MemberId = member.Id
                }));

            Assert.Contains("maximum", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ── ReturnBookAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task ReturnBookAsync_SucceedsAndIncrementsAvailableCopies()
        {
            // Arrange
            var context = CreateDbContext();
            var book = MakeBook(availableCopies: 2, totalCopies: 3);
            var member = MakeMember();

            context.Books.Add(book);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            var borrowRecord = new BorrowRecord
            {
                Id = Guid.NewGuid(),
                BookId = book.Id,
                MemberId = member.Id,
                BorrowDate = DateTime.UtcNow.AddDays(-3),
                Status = "Borrowed"
            };

            context.BorrowRecords.Add(borrowRecord);
            await context.SaveChangesAsync();

            var bookRepoMock = new Mock<IBookRepository>();
            bookRepoMock.Setup(r => r.GetById(book.Id)).ReturnsAsync(book);

            var memberRepoMock = new Mock<IMemberRepository>();
            var borrowRepoMock = new Mock<IBorrowRecordRepository>();
            borrowRepoMock.Setup(r => r.GetById(borrowRecord.Id)).ReturnsAsync(borrowRecord);

            var service = CreateService(context, borrowRepoMock, bookRepoMock, memberRepoMock);

            // Act
            var result = await service.ReturnBookAsync(new ReturnRequest
            {
                BorrowRecordId = borrowRecord.Id
            });

            // Assert
            Assert.Equal("Returned", result.Status);
            Assert.NotNull(result.ReturnDate);
            Assert.Equal(3, book.AvailableCopies); // incremented from 2 back to 3
        }

        [Fact]
        public async Task ReturnBookAsync_Throws_WhenBorrowRecordNotFound()
        {
            // Arrange
            var context = CreateDbContext();
            var bookRepoMock = new Mock<IBookRepository>();
            var memberRepoMock = new Mock<IMemberRepository>();
            var borrowRepoMock = new Mock<IBorrowRecordRepository>();
            borrowRepoMock.Setup(r => r.GetById(It.IsAny<Guid>()))
                          .ReturnsAsync((BorrowRecord?)null);

            var service = CreateService(context, borrowRepoMock, bookRepoMock, memberRepoMock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ReturnBookAsync(new ReturnRequest { BorrowRecordId = Guid.NewGuid() }));
        }

        [Fact]
        public async Task ReturnBookAsync_Throws_WhenBookAlreadyReturned()
        {
            // Arrange — borrow record is already in "Returned" state
            var context = CreateDbContext();
            var book = MakeBook();
            var member = MakeMember();

            context.Books.Add(book);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            var borrowRecord = new BorrowRecord
            {
                Id = Guid.NewGuid(),
                BookId = book.Id,
                MemberId = member.Id,
                BorrowDate = DateTime.UtcNow.AddDays(-7),
                ReturnDate = DateTime.UtcNow.AddDays(-1),
                Status = "Returned"  // ← already returned
            };

            context.BorrowRecords.Add(borrowRecord);
            await context.SaveChangesAsync();

            var bookRepoMock = new Mock<IBookRepository>();
            var memberRepoMock = new Mock<IMemberRepository>();
            var borrowRepoMock = new Mock<IBorrowRecordRepository>();
            borrowRepoMock.Setup(r => r.GetById(borrowRecord.Id)).ReturnsAsync(borrowRecord);

            var service = CreateService(context, borrowRepoMock, bookRepoMock, memberRepoMock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ReturnBookAsync(new ReturnRequest { BorrowRecordId = borrowRecord.Id }));
        }

        [Fact]
        public async Task ReturnBookAsync_DoesNotExceedTotalCopies()
        {
            // Arrange — AvailableCopies already equals TotalCopies (edge case guard)
            var context = CreateDbContext();
            var book = MakeBook(availableCopies: 3, totalCopies: 3); // already at max
            var member = MakeMember();

            context.Books.Add(book);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            var borrowRecord = new BorrowRecord
            {
                Id = Guid.NewGuid(),
                BookId = book.Id,
                MemberId = member.Id,
                BorrowDate = DateTime.UtcNow.AddDays(-2),
                Status = "Borrowed"
            };

            context.BorrowRecords.Add(borrowRecord);
            await context.SaveChangesAsync();

            var bookRepoMock = new Mock<IBookRepository>();
            bookRepoMock.Setup(r => r.GetById(book.Id)).ReturnsAsync(book);

            var memberRepoMock = new Mock<IMemberRepository>();
            var borrowRepoMock = new Mock<IBorrowRecordRepository>();
            borrowRepoMock.Setup(r => r.GetById(borrowRecord.Id)).ReturnsAsync(borrowRecord);

            var service = CreateService(context, borrowRepoMock, bookRepoMock, memberRepoMock);

            // Act
            await service.ReturnBookAsync(new ReturnRequest { BorrowRecordId = borrowRecord.Id });

            // Assert — AvailableCopies should NOT exceed TotalCopies
            Assert.True(book.AvailableCopies <= book.TotalCopies);
        }

        // ── GetAllBorrowRecordsAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetAllBorrowRecordsAsync_ReturnsAllRecords()
        {
            // Arrange
            var context = CreateDbContext();
            var records = new List<BorrowRecord>
            {
                new BorrowRecord { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), MemberId = Guid.NewGuid(), BorrowDate = DateTime.UtcNow, Status = "Borrowed" },
                new BorrowRecord { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), MemberId = Guid.NewGuid(), BorrowDate = DateTime.UtcNow, Status = "Returned" }
            };

            var borrowRepoMock = new Mock<IBorrowRecordRepository>();
            borrowRepoMock.Setup(r => r.GetAll()).ReturnsAsync(records);

            var service = CreateService(context, borrowRepoMock, new Mock<IBookRepository>(), new Mock<IMemberRepository>());

            // Act
            var result = await service.GetAllBorrowRecordsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        // ── GetMemberBorrowHistoryAsync ───────────────────────────────────────────

        [Fact]
        public async Task GetMemberBorrowHistoryAsync_Throws_WhenMemberNotFound()
        {
            // Arrange
            var context = CreateDbContext();
            var memberRepoMock = new Mock<IMemberRepository>();
            memberRepoMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Member?)null);

            var service = CreateService(context, new Mock<IBorrowRecordRepository>(),
                new Mock<IBookRepository>(), memberRepoMock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetMemberBorrowHistoryAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetMemberBorrowHistoryAsync_ReturnsOnlyMembersRecords()
        {
            // Arrange
            var context = CreateDbContext();
            var member = MakeMember();
            var memberId = member.Id;

            var memberRecords = new List<BorrowRecord>
            {
                new BorrowRecord { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), MemberId = memberId, BorrowDate = DateTime.UtcNow, Status = "Borrowed" },
                new BorrowRecord { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), MemberId = memberId, BorrowDate = DateTime.UtcNow, Status = "Returned" }
            };

            var memberRepoMock = new Mock<IMemberRepository>();
            memberRepoMock.Setup(r => r.GetById(memberId)).ReturnsAsync(member);

            var borrowRepoMock = new Mock<IBorrowRecordRepository>();
            borrowRepoMock.Setup(r => r.GetByMemberId(memberId)).ReturnsAsync(memberRecords);

            var service = CreateService(context, borrowRepoMock, new Mock<IBookRepository>(), memberRepoMock);

            // Act
            var result = await service.GetMemberBorrowHistoryAsync(memberId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Equal(memberId, r.MemberId));
        }
    }
}
