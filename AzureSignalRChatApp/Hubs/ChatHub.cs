using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureSignalRChatApp.Hubs
{
    public class ChatHub: Hub
    {
        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, message);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public void Echo(string name, string message)
        {
            Clients.Client(Context.ConnectionId).SendAsync("echo", name, message + " (echo from server)");
        }
    }
}
