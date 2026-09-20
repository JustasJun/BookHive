namespace BookHive.Models;

public class LibraryBook
{
    public Book Book { get; set; }

    public ReadingStatus Status { get; set; }

    public bool IsFavorite { get; set; }

    public LibraryBook(Book book, ReadingStatus status)
    {
        Book = book;
        Status = status;
        IsFavorite = false;
    }
}

// kai atsiras vartotojai, pridet reiks ir id