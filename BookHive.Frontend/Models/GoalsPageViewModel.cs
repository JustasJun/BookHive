using BookHive.Models;

namespace BookHive.Frontend.Models;

public class GoalsPageViewModel
{
    public GoalProgress? Current { get; set; }
    public List<GoalHistoryItem> History { get; set; } = new();
}
