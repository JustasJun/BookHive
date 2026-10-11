namespace BookHive.Models;

public class ReadingGoal
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public GoalPeriod Period { get; set; }
    public int TargetBooks { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndsAt { get; set; }

    public GoalStatus Status { get; set; }
    public int BooksRead { get; set; }        // frozen when the goal finishes
    public DateTime? FinishedAt { get; set; }

    public ReadingGoal(int userId, GoalPeriod period, int targetBooks)
    {
        UserId = userId;
        Period = period;
        TargetBooks = targetBooks;
        StartedAt = DateTime.UtcNow;
        EndsAt = period.AddTo(StartedAt);
        Status = GoalStatus.Active;
    }
}