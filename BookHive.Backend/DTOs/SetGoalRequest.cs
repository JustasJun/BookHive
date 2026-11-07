using BookHive.Models;

namespace BookHive.Backend.DTOs;

public record SetGoalRequest(
    GoalPeriod Period,
    int TargetBooks);
