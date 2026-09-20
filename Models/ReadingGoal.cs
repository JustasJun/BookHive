namespace BookHive.Models;

public record ReadingGoal(
    int UserId, 
    int Year, 
    int TargetBooks);