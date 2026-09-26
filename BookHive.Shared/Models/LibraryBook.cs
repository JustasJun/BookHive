namespace BookHive.Models;

public class LibraryBook
{
    public int UserId { get; set; }
    
    public Book Book { get; set; }

    public ReadingStatus Status { get; set; }

    public bool IsFavorite { get; set; }

    public LibraryBook(int userId, Book book, ReadingStatus status)
    {
        UserId = userId;
        Book = book;
        Status = status;
        IsFavorite = false;
    }
}

// kai atsiras vartotojai, pridet reiks ir id