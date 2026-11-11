using BookHive.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Tests.Helpers;

public static class TestDbContextFactory
{
    public static BookHiveDbContext Create()
    {
        DbContextOptions<BookHiveDbContext> options =
            new DbContextOptionsBuilder<BookHiveDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new BookHiveDbContext(options);
    }
}