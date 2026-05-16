using Microsoft.AspNetCore.SignalR;

namespace FeedbackBoard.Api.Hubs;

public class FeedbackHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "FeedbackWatchers");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "FeedbackWatchers");
        await base.OnDisconnectedAsync(exception);
    }
}