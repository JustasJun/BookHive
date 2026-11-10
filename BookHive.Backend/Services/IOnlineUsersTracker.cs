namespace BookHive.Services;

public interface IOnlineUsersTracker
{
    int OnlineCount { get; }
    void Connect(string connectionId, int userId);
    void Disconnect(string connectionId);
}