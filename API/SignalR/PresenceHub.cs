using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Extansions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;

        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var IsOnline= await _tracker.UserConnected(Context.User.GetuserName(), Context.ConnectionId);
            if(IsOnline){
            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetuserName());
            }
            var currentUsers = await _tracker.GetOnlinUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
           var isOffline= await _tracker.UserDisconnected(Context.User.GetuserName(), Context.ConnectionId);
           if(isOffline){
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetuserName());
           }
            await base.OnDisconnectedAsync(exception);
          }
    }
}