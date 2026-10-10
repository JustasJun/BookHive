using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface ILibraryRepository
{
    bool BookExists(int userId, string bookId);

    LibraryBook? FindBook(int userId, string bookId);

    List<LibraryBook> GetBooks(int userId);

    void Add(LibraryBook libraryBook);

    void Remove(LibraryBook libraryBook);

    int CountReadBooks(int userId, DateTime from, DateTime to);

    void Save();
}
