using BookHive.Backend.Data;
using BookHive.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Backend.Repositories;

public class LibraryRepository : ILibraryRepository
{
    private readonly BookHiveDbContext _context;

    public LibraryRepository(BookHiveDbContext context)
    {
        _context = context;
    }

    public async Task<bool> BookExistsAsync(int userId, string bookId)
    {
        return await _context.LibraryBooks.AnyAsync(item =>
            item.UserId == userId &&
            item.OpenLibraryBookId == bookId);
    }

    public async Task<LibraryBook?> FindBookAsync(int userId, string bookId)
    {
        return await _context.LibraryBooks.FirstOrDefaultAsync(item =>
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
        return await _context.LibraryBooks
            .Where(item => item.UserId == userId)
            .Select(item => item.OpenLibraryBookId)
            .ToListAsync();
    }

    public void Add(LibraryBook libraryBook)
    {
        _context.LibraryBooks.Add(libraryBook);
    }

    public void Remove(LibraryBook libraryBook)
    {
        _context.LibraryBooks.Remove(libraryBook);
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

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}