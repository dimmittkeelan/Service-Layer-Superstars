using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Seeder
{
    /// <summary>
    /// Seeds the in-memory database with initial demo data on application startup.
    /// This makes the API immediately usable for testing and demos without requiring
    /// manual POST requests to create books and members first.
    /// </summary>
    public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Only seed if the database is empty to avoid duplicate data on restart
            if (await context.Books.AnyAsync() || await context.Members.AnyAsync())
                return;

            // ── Seed Books ────────────────────────────────────────────────────────────

            var books = new List<Book>
            {
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    ISBN = "978-0132350884",
                    TotalCopies = 5,
                    AvailableCopies = 5
                },
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "The Pragmatic Programmer",
                    Author = "David Thomas, Andrew Hunt",
                    ISBN = "978-0135957059",
                    TotalCopies = 3,
                    AvailableCopies = 3
                },
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Design Patterns: Elements of Reusable Object-Oriented Software",
                    Author = "Gang of Four",
                    ISBN = "978-0201633610",
                    TotalCopies = 4,
                    AvailableCopies = 4
                },
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "You Don't Know JS",
                    Author = "Kyle Simpson",
                    ISBN = "978-1491904244",
                    TotalCopies = 6,
                    AvailableCopies = 6
                },
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Introduction to Algorithms",
                    Author = "Cormen, Leiserson, Rivest, Stein",
                    ISBN = "978-0262046305",
                    TotalCopies = 2,
                    AvailableCopies = 1
                }
            };

            // ── Seed Members ──────────────────────────────────────────────────────────

            var members = new List<Member>
            {
                new Member
                {
                    Id = Guid.NewGuid(),
                    FullName = "Alice Johnson",
                    Email = "alice.johnson@example.com",
                    MembershipDate = new DateTime(2024, 9, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Member
                {
                    Id = Guid.NewGuid(),
                    FullName = "Bob Martinez",
                    Email = "bob.martinez@example.com",
                    MembershipDate = new DateTime(2024, 10, 15, 0, 0, 0, DateTimeKind.Utc)
                },
                new Member
                {
                    Id = Guid.NewGuid(),
                    FullName = "Carol Lee",
                    Email = "carol.lee@example.com",
                    MembershipDate = new DateTime(2025, 1, 20, 0, 0, 0, DateTimeKind.Utc)
                }
            };

            await context.Books.AddRangeAsync(books);
            await context.Members.AddRangeAsync(members);

            // ── Seed a sample BorrowRecord ────────────────────────────────────────────
            // Alice has borrowed "Introduction to Algorithms" (the one with AvailableCopies = 1)
            var algoBook = books.First(b => b.ISBN == "978-0262046305");
            var alice = members.First(m => m.Email == "alice.johnson@example.com");

            var borrowRecord = new BorrowRecord
            {
                Id = Guid.NewGuid(),
                BookId = algoBook.Id,
                MemberId = alice.Id,
                BorrowDate = new DateTime(2026, 4, 20, 9, 0, 0, DateTimeKind.Utc),
                ReturnDate = null,
                Status = "Borrowed"
            };

            await context.BorrowRecords.AddAsync(borrowRecord);
            await context.SaveChangesAsync();
        }
    }
}
