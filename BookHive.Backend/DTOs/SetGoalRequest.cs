using BookHive.Models;

namespace BookHive.Backend.DTOs;

public record SetGoalRequest(
    int UserId,
    GoalPeriod Period,
    int TargetBooks);