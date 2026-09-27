using BookHive.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Backend.Data;

public class BookHiveDbContext : DbContext
{
    public BookHiveDbContext(DbContextOptions<BookHiveDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<LibraryBook> LibraryBooks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LibraryBook>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(libraryBook => libraryBook.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LibraryBook>()
            .HasIndex(libraryBook => new
            {
                libraryBook.UserId,
                libraryBook.OpenLibraryBookId
            })
            .IsUnique();
    }
}