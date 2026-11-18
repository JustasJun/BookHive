using BookHive.Backend.Data;
using BookHive.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Backend.Repositories;

public class LibraryRepository : Repository<LibraryBook>, ILibraryRepository
{
    public LibraryRepository(BookHiveDbContext context) : base(context)
    {
    }

    public async Task<bool> BookExistsAsync(int userId, string bookId)
    {
        return await ExistsAsync(item =>
            item.UserId == userId &&
            item.OpenLibraryBookId == bookId);
    }

    public async Task<LibraryBook?> FindBookAsync(int userId, string bookId)
    {
        return await FindFirstAsync(item =>
            item.UserId == userId &&
            item.OpenLibraryBookId == bookId);
    }

    public async Task<List<LibraryBook>> GetBooksAsync(int userId)
    {
        return await _context.LibraryBooks
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.Id)
            .ToListAsync();
    }

    public async Task<List<string>> GetBookIdsAsync(int userId)
    {
        return await SelectAsync(
            item => item.UserId == userId,
            item => item.OpenLibraryBookId);
    }

    public async Task<int> CountReadBooksAsync(
        int userId, DateTime from, DateTime to)
    {
        return await _context.LibraryBooks.CountAsync(item =>
            item.UserId == userId &&
            item.Status == ReadingStatus.Read &&
            item.ReadAt != null &&
            item.ReadAt >= from &&
            item.ReadAt < to);
    }
}
