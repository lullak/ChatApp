using ChatApp.Core.Interfaces.Repos;
using ChatApp.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChatApp.Hubs
{
    [Authorize]
    public class ChatHub(IChatRepo chatRepo) : Hub
    { 
        private readonly IChatRepo _chatRepo = chatRepo;
        private static readonly ConcurrentDictionary<string, string> OnlineUsers = new();
        public static bool IsUserOnline(string username) => OnlineUsers.ContainsKey(username);

        public async Task SendMessage(string username, string message)
        {
            var chatMessage = ChatMessage.Create(username, message);
            await _chatRepo.SaveMessageAsync(chatMessage);
            await Clients.All.SendAsync("ReceiveMessage", username, message);
        }
    }
}
