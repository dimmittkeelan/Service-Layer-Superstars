using LibraryApi.Dtos;
using LibraryApi.Models;
using LibraryApi.Repositories;
using LibraryApi.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LibraryApi.Tests.Services
{
    public class BookServiceTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────────

        private static IMemoryCache CreateCache() =>
            new MemoryCache(new MemoryCacheOptions());

        private static Book MakeBook(
            string title = "Test Book",
            string author = "Test Author",
            string isbn = "123-456",
            int totalCopies = 5,
            int availableCopies = 5) => new Book
            {
                Id = Guid.NewGuid(),
                Title = title,
                Author = author,
                ISBN = isbn,
                TotalCopies = totalCopies,
                AvailableCopies = availableCopies
            };

        private static BookService CreateService(
            Mock<IBookRepository> repoMock,
            IMemoryCache? cache = null) =>
            new BookService(
                repoMock.Object,
                cache ?? CreateCache(),
                NullLogger<BookService>.Instance);

        // ── GetBooksAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetBooksAsync_ReturnsAllBooks_WhenNoSearchTerm()
        {
            // Arrange
            var books = new List<Book>
            {
                MakeBook("Clean Code", "Robert Martin", "111"),
                MakeBook("The Pragmatic Programmer", "David Thomas", "222")
            };

            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetAll()).ReturnsAsync(books);

            var service = CreateService(repoMock);

            // Act
            var result = await service.GetBooksAsync();

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count());
        }

        [Fact]
        public async Task GetBooksAsync_FiltersResults_WhenSearchTermProvided()
        {
            // Arrange
            var books = new List<Book>
            {
                MakeBook("Clean Code", "Robert Martin", "111"),
                MakeBook("Design Patterns", "Gang of Four", "222")
            };

            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetAll()).ReturnsAsync(books);

            var service = CreateService(repoMock);

            // Act
            var result = await service.GetBooksAsync(search: "clean");

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Clean Code", result.Items.First().Title);
        }

        [Fact]
        public async Task GetBooksAsync_SearchIsCaseInsensitive()
        {
            // Arrange
            var books = new List<Book>
            {
                MakeBook("Clean Code", isbn: "111"),
                MakeBook("Design Patterns", isbn: "222")
            };

            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetAll()).ReturnsAsync(books);

            var service = CreateService(repoMock);

            // Act — uppercase search should still match lowercase title
            var result = await service.GetBooksAsync(search: "CLEAN");

            // Assert
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetBooksAsync_ReturnsCorrectPage()
        {
            // Arrange
            var books = Enumerable.Range(1, 15)
                .Select(i => MakeBook($"Book {i}", isbn: $"ISBN-{i}"))
                .ToList();

            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetAll()).ReturnsAsync(books);

            var service = CreateService(repoMock);

            // Act — page 2, 10 items per page → should get items 11-15
            var result = await service.GetBooksAsync(page: 2, pageSize: 10);

            // Assert
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(5, result.Items.Count());
        }

        [Fact]
        public async Task GetBooksAsync_ServesFromCache_OnSecondCall()
        {
            // Arrange
            var books = new List<Book> { MakeBook(isbn: "111") };
            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetAll()).ReturnsAsync(books);

            var service = CreateService(repoMock);

            // Act — call twice
            await service.GetBooksAsync();
            await service.GetBooksAsync();

            // Assert — repository should only be called ONCE (second call hits cache)
            repoMock.Verify(r => r.GetAll(), Times.Once);
        }

        // ── GetBookByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetBookByIdAsync_ReturnsBook_WhenExists()
        {
            // Arrange
            var book = MakeBook("Clean Code", isbn: "111");
            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetById(book.Id)).ReturnsAsync(book);

            var service = CreateService(repoMock);

            // Act
            var result = await service.GetBookByIdAsync(book.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(book.Id, result.Id);
            Assert.Equal("Clean Code", result.Title);
        }

        [Fact]
        public async Task GetBookByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Book?)null);

            var service = CreateService(repoMock);

            // Act
            var result = await service.GetBookByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetBookByIdAsync_ServesFromCache_OnSecondCall()
        {
            // Arrange
            var book = MakeBook(isbn: "111");
            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetById(book.Id)).ReturnsAsync(book);

            var service = CreateService(repoMock);

            // Act — call twice
            await service.GetBookByIdAsync(book.Id);
            await service.GetBookByIdAsync(book.Id);

            // Assert — repository hit only once
            repoMock.Verify(r => r.GetById(book.Id), Times.Once);
        }

        // ── CreateBookAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateBookAsync_ReturnsCreatedBook()
        {
            // Arrange
            var request = new CreateBookRequest
            {
                Title = "New Book",
                Author = "New Author",
                ISBN = "999-999",
                TotalCopies = 3,
                AvailableCopies = 3
            };

            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.Add(It.IsAny<Book>()))
                    .ReturnsAsync((Book b) => b);

            var service = CreateService(repoMock);

            // Act
            var result = await service.CreateBookAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Book", result.Title);
            Assert.Equal("New Author", result.Author);
            Assert.Equal(3, result.TotalCopies);
        }

        [Fact]
        public async Task CreateBookAsync_ThrowsArgumentException_WhenTotalCopiesIsZero()
        {
            // Arrange
            var request = new CreateBookRequest
            {
                Title = "Bad Book",
                Author = "Author",
                ISBN = "000",
                TotalCopies = 0,         // ← invalid
                AvailableCopies = 0
            };

            var repoMock = new Mock<IBookRepository>();
            var service = CreateService(repoMock);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateBookAsync(request));
        }

        [Fact]
        public async Task CreateBookAsync_ThrowsArgumentException_WhenAvailableExceedsTotal()
        {
            // Arrange
            var request = new CreateBookRequest
            {
                Title = "Bad Book",
                Author = "Author",
                ISBN = "000",
                TotalCopies = 2,
                AvailableCopies = 5      // ← available > total
            };

            var repoMock = new Mock<IBookRepository>();
            var service = CreateService(repoMock);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateBookAsync(request));
        }

        [Fact]
        public async Task CreateBookAsync_ThrowsArgumentException_WhenAvailableCopiesIsNegative()
        {
            // Arrange
            var request = new CreateBookRequest
            {
                Title = "Bad Book",
                Author = "Author",
                ISBN = "000",
                TotalCopies = 3,
                AvailableCopies = -1     // ← negative
            };

            var repoMock = new Mock<IBookRepository>();
            var service = CreateService(repoMock);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateBookAsync(request));
        }

        // ── UpdateBookAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateBookAsync_UpdatesTitle_WhenProvided()
        {
            // Arrange
            var existing = MakeBook("Old Title", isbn: "111");
            var request = new UpdateBookRequest { Title = "New Title" };

            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetById(existing.Id)).ReturnsAsync(existing);
            repoMock.Setup(r => r.Update(It.IsAny<Book>())).Returns(Task.CompletedTask);

            var service = CreateService(repoMock);

            // Act
            var result = await service.UpdateBookAsync(existing.Id, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Title", result.Title);
        }

        [Fact]
        public async Task UpdateBookAsync_ReturnsNull_WhenBookNotFound()
        {
            // Arrange
            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Book?)null);

            var service = CreateService(repoMock);

            // Act
            var result = await service.UpdateBookAsync(Guid.NewGuid(), new UpdateBookRequest());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateBookAsync_ThrowsArgumentException_WhenAvailableWouldExceedTotal()
        {
            // Arrange
            var existing = MakeBook(totalCopies: 3, availableCopies: 2, isbn: "111");
            var request = new UpdateBookRequest { AvailableCopies = 10 }; // ← would exceed total

            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetById(existing.Id)).ReturnsAsync(existing);

            var service = CreateService(repoMock);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateBookAsync(existing.Id, request));
        }

        // ── DeleteBookAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteBookAsync_ReturnsDeletedBook_WhenExists()
        {
            // Arrange
            var book = MakeBook(isbn: "111");
            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetById(book.Id)).ReturnsAsync(book);
            repoMock.Setup(r => r.Delete(book.Id)).Returns(Task.CompletedTask);

            var service = CreateService(repoMock);

            // Act
            var result = await service.DeleteBookAsync(book.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(book.Id, result.Id);
            repoMock.Verify(r => r.Delete(book.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteBookAsync_ReturnsNull_WhenBookNotFound()
        {
            // Arrange
            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Book?)null);

            var service = CreateService(repoMock);

            // Act
            var result = await service.DeleteBookAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteBookAsync_InvalidatesCache_AfterDelete()
        {
            // Arrange — pre-populate the cache by calling GetAll first
            var book = MakeBook(isbn: "111");
            var repoMock = new Mock<IBookRepository>();
            repoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<Book> { book });
            repoMock.Setup(r => r.GetById(book.Id)).ReturnsAsync(book);
            repoMock.Setup(r => r.Delete(book.Id)).Returns(Task.CompletedTask);

            var cache = CreateCache();
            var service = CreateService(repoMock, cache);

            // Warm up the cache
            await service.GetBooksAsync();

            // Act — delete should invalidate the cache
            await service.DeleteBookAsync(book.Id);

            // Next GetAll call should hit the repo again (not the cache)
            await service.GetBooksAsync();

            // Assert — repo.GetAll called twice: once before delete, once after
            repoMock.Verify(r => r.GetAll(), Times.Exactly(2));
        }
    }
}
