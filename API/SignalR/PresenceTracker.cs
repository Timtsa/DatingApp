using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, List<string>> OnLineUsers = new
        Dictionary<string, List<string>>();

        public Task<bool> UserConnected(string userName, string connectionId)
        {
            bool IsOnline = false;
            lock (OnLineUsers)
            {
                if (OnLineUsers.ContainsKey(userName))
                {
                    OnLineUsers[userName].Add(connectionId);
                }
                else
                {
                    OnLineUsers.Add(userName, new List<string> { connectionId });
                    IsOnline =true;
                }
            }
            return Task.FromResult(IsOnline);
        }

        public Task<bool> UserDisconnected(string userName, string connectionId)
        {
            bool isOffline=false;
            lock (OnLineUsers)
            {

                if (!OnLineUsers.ContainsKey(userName)) return Task.FromResult(isOffline);
                OnLineUsers[userName].Remove(connectionId);
                if (OnLineUsers[userName].Count == 0){
                    OnLineUsers.Remove(userName);
                   isOffline=true;
                }
            }
            return Task.FromResult(isOffline);
        }
        public Task<string[]> GetOnlinUsers()
        {
            string[] onlineUsers;
            lock (OnLineUsers)
            {
                onlineUsers = OnLineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }

            return Task.FromResult(onlineUsers);
        }

        public Task<List<string>> GetConnectionsForUser(string userName)
        {
            List<string> connectionsIds;
            lock (OnLineUsers)
            {
                connectionsIds = OnLineUsers.GetValueOrDefault(userName);
            }
            return Task.FromResult(connectionsIds);
        }

    }
}