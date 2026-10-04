namespace BookHive.Models;

public class Review : IComparable<Review>
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string OpenLibraryBookId { get; set; }
    public string BookTitle { get; set; }
    public string BookAuthor { get; set; }
    public string? CoverId { get; set; }
    public int Rating { get; set; }          // 1-5
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }

    public Review(
        int userId,
        string openLibraryBookId,
        string bookTitle,
        string bookAuthor,
        string? coverId,
        int rating,
        string text)
    {
        UserId = userId;
        OpenLibraryBookId = openLibraryBookId;
        BookTitle = bookTitle;
        BookAuthor = bookAuthor;
        CoverId = coverId;
        Rating = rating;
        Text = text;
        CreatedAt = DateTime.UtcNow;
    }

    public int CompareTo(Review? other)
    {
        if (other == null)
        {
            return 1;
        }

        return Rating.CompareTo(other.Rating);
    }
}