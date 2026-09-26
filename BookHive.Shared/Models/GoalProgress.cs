namespace BookHive.Models;

public record GoalProgress(
    int Year,
    int TargetBooks,
    int BooksRead,
    double Percentage,
    int ExpectedByNow, // How many books should have been finished by today
    int PaceDifference, // positive = ahead of schedule, negative = behind
    bool IsCompleted);