namespace BookHive.Models;

public record GoalProgress(
    GoalPeriod Period,
    DateTime StartedAt,
    DateTime EndsAt,
    int DaysLeft,
    int TargetBooks,
    int BooksRead,
    double Percentage,
    int ExpectedByNow, // How many books should have been finished by now
    int PaceDifference, // positive = ahead of schedule, negative = behind
    bool IsCompleted,
    GoalStatus Status);