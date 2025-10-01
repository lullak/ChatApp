using ChatApp.Core.Interfaces.Repos;
using ChatApp.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChatApp.Hubs
{
    [Authorize]
    public class ChatHub(IChatRepo chatRepo, ILogger logger) : Hub
    { 
        private readonly IChatRepo _chatRepo = chatRepo;
        private readonly ILogger _logger = logger;
        private static readonly ConcurrentDictionary<string, string> OnlineUsers = new();
        public static bool IsUserOnline(string username) => OnlineUsers.ContainsKey(username);

        public async Task SendMessage(string username, string message)
        {
            var chatMessage = ChatMessage.Create(username, message);
            await _chatRepo.SaveMessageAsync(chatMessage);
            _logger.LogInformation($"Message saved, username: {username}, messageId = {chatMessage.Id}");
            await Clients.All.SendAsync("ReceiveMessage", username, message);
            _logger.LogInformation($"ReceiveMessage, clients.all {username} ConnectionId={Context.ConnectionId}");
        }
    }
}
