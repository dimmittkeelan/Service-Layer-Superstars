using LibraryApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<BorrowRecord> BorrowRecords => Set<BorrowRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasIndex(b => b.ISBN).IsUnique();
            entity.Property(b => b.RowVersion).IsRowVersion();
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Books_TotalCopies", "\"TotalCopies\" > 0");
                t.HasCheckConstraint("CK_Books_AvailableCopies_Range", "\"AvailableCopies\" >= 0 AND \"AvailableCopies\" <= \"TotalCopies\"");
            });
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasIndex(m => m.Email).IsUnique();
            entity.Property(m => m.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<BorrowRecord>(entity =>
        {
            entity.Property(br => br.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(br => br.Book)
                .WithMany(b => b.BorrowRecords)
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(br => br.Member)
                .WithMany(m => m.BorrowRecords)
                .HasForeignKey(br => br.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
