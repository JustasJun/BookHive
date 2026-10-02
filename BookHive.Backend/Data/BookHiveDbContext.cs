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

    public DbSet<Review> Reviews { get; set; }

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

        modelBuilder.Entity<Review>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(review => review.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // vienas vartotojas gali parašyti vieną atsiliepimą apie knygą
        modelBuilder.Entity<Review>()
            .HasIndex(review => new { review.UserId, review.OpenLibraryBookId })
            .IsUnique();
    }
}