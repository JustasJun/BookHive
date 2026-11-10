using BookHive.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BookHive.Backend.Hubs;

[Authorize]
public class PresenceHub : Hub
{
    private const string OnlineCountChanged = "OnlineCountChanged";

    private readonly IOnlineUsersTracker _tracker;

    public PresenceHub(IOnlineUsersTracker tracker)
    {
        _tracker = tracker;
    }

    public override async Task OnConnectedAsync()
    {
        if (int.TryParse(Context.UserIdentifier, out int userId))
        {
            _tracker.Connect(Context.ConnectionId, userId);
        }

        await Clients.All.SendAsync(OnlineCountChanged, _tracker.OnlineCount);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _tracker.Disconnect(Context.ConnectionId);

        await Clients.All.SendAsync(OnlineCountChanged, _tracker.OnlineCount);
        await base.OnDisconnectedAsync(exception);
    }
}