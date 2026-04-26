using Microsoft.EntityFrameworkCore;
using LibraryApi.Models;

namespace LibraryApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>
        options) : base(options) { }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<BorrowRecord> BorrowRecords => Set<BorrowRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .ToTable(tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint("CK_Books_TotalCopies_Positive", "TotalCopies > 0");
                    tableBuilder.HasCheckConstraint("CK_Books_AvailableCopies_NonNegative", "AvailableCopies >= 0");
                    tableBuilder.HasCheckConstraint("CK_Books_AvailableCopies_NotGreaterThanTotal", "AvailableCopies <= TotalCopies");
                });
        }
    }
}