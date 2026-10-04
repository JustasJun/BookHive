namespace BookHive.Models;

public class ReadingGoal
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Year { get; set; }
    public int TargetBooks { get; set; }
    public DateTime StartedAt { get; set; }

    public ReadingGoal(int userId, int year, int targetBooks)
    {
        UserId = userId;
        Year = year;
        TargetBooks = targetBooks;
        StartedAt = DateTime.UtcNow;
    }
}