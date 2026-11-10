using System.Collections.Concurrent;

namespace BookHive.Services;

public class OnlineUsersTracker : IOnlineUsersTracker
{
    private readonly ConcurrentDictionary<string, int> _connections = new();

    public int OnlineCount => _connections.Values.Distinct().Count();

    public void Connect(string connectionId, int userId)
    {
        _connections[connectionId] = userId;
    }

    public void Disconnect(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }
}