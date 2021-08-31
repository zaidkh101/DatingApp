using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker tracker;

        public PresenceHub(PresenceTracker tracker)
        {

            this.tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {

            await tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);

            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());

            var CurrentUsers = await tracker.GetOnlineUsers();

            await Clients.All.SendAsync("GetOnlineUsers", CurrentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
            var CurrentUsers = await tracker.GetOnlineUsers();

            await Clients.All.SendAsync("GetOnlineUsers", CurrentUsers);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
