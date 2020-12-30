using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

using ChatRedisApp.Models;
using StackExchange.Redis;
using Microsoft.AspNetCore.SignalR.StackExchangeRedis;

namespace ChatRedisApp.Hubs
{
    public class ChatHub : Hub
    {
  
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

    }
}
