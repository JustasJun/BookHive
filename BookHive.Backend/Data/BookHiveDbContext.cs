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
    public DbSet<ReadingGoal> ReadingGoals { get; set; }

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

        // one user can write one review for a book
        modelBuilder.Entity<Review>()
            .HasIndex(review => new { review.UserId, review.OpenLibraryBookId })
            .IsUnique();

        modelBuilder.Entity<ReadingGoal>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(goal => goal.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // many goals per user (history), but only one active goal at a time
        modelBuilder.Entity<ReadingGoal>()
            .HasIndex(goal => goal.UserId)
            .IsUnique()
            .HasFilter("\"Status\" = 0");

        // follow: many-to-many between users, stored in the "Follows" join table
        modelBuilder.Entity<User>()
            .HasMany(user => user.Following)
            .WithMany(user => user.Followers)
            .UsingEntity<Dictionary<string, object>>("Follows", followee => followee
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("FolloweeId")
                    .OnDelete(DeleteBehavior.Cascade),
                follower => follower
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("FollowerId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.HasKey("FollowerId", "FolloweeId");
                    join.HasIndex("FolloweeId");
                    join.ToTable("Follows", table => table.HasCheckConstraint(
                        "CK_Follows_NoSelfFollow",
                        "\"FollowerId\" <> \"FolloweeId\""));
                });
    }
}
