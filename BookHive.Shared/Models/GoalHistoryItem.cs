namespace BookHive.Models;

public record GoalHistoryItem(
    int Id,
    GoalPeriod Period,
    int TargetBooks,
    int BooksRead,
    GoalStatus Status,
    DateTime StartedAt,
    DateTime FinishedAt);
