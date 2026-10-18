namespace BookHive.Models;

public class LibraryBook
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string OpenLibraryBookId { get; set; }
    public ReadingStatus Status { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? ReadAt { get; set; }

    public LibraryBook(
        int userId,
        string openLibraryBookId,
        ReadingStatus status)
    {
        UserId = userId;
        OpenLibraryBookId = openLibraryBookId;
        Status = status;
        IsFavorite = false;
        ReadAt = status == ReadingStatus.Read ? DateTime.UtcNow : null;
    }
}
