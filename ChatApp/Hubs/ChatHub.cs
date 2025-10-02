using ChatApp.Core.Interfaces.Repos;
using ChatApp.Core.Interfaces.Services;
using ChatApp.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChatApp.Hubs
{
    [Authorize]
    public class ChatHub(IChatRepo chatRepo, ILogger<ChatHub> logger, IAesKeyService aesKeyService) : Hub
    {
        private readonly IChatRepo _chatRepo = chatRepo;
        private readonly ILogger<ChatHub> _logger = logger;
        private readonly IAesKeyService _aes = aesKeyService;

        private static readonly ConcurrentDictionary<string, string> GroupKeys = new();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> OnlineUsers = new();

        // Delar har tagits fram i samband med Ai, speciellt kring hantering av privata grupper
        public async Task SendMessageAll(string message)
        {
            var username = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("OnConnectedAsync: Missing username, disconnecting.");
                Context.Abort();
                return;
            }

            var chatMessage = ChatMessage.Create(username, message);


            await _chatRepo.SaveMessageAsync(chatMessage);
            _logger.LogInformation($"Message saved, username: {username}, messageId = {chatMessage.Id}");
            await Clients.All.SendAsync("ReceiveMessage", username, message);
            _logger.LogInformation($"ReceiveMessage, clients.all {username} ConnectionId={Context.ConnectionId}");
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("OnConnectedAsync: Missing username, disconnecting.");
                Context.Abort();
                return;
            }

            var connections = OnlineUsers.GetOrAdd(username, _ => new ConcurrentDictionary<string, byte>());
            connections[Context.ConnectionId] = 0;
            _logger.LogInformation($"User {username} connected with ConnectionId {Context.ConnectionId}");

            var generalKeyBase64 = GroupKeys.GetOrAdd("General", _ => Convert.ToBase64String(_aes.GenerateRandomKey()));

            await Clients.Client(Context.ConnectionId).SendAsync("GeneralChatKey", generalKeyBase64);
            await Clients.All.SendAsync("UpdateUserList", OnlineUsers.Keys.ToArray());
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(username) && OnlineUsers.TryGetValue(username, out var connections))
            {
                connections.TryRemove(Context.ConnectionId, out _);
                if (connections.IsEmpty)
                {
                    OnlineUsers.TryRemove(username, out _);
                    _logger.LogInformation($"User {username} disconnected (no remaining connections).");
                }
                else
                {
                    _logger.LogInformation($"User {username} disconnected connection {Context.ConnectionId} (still has other connections).");
                }
            }

            if (exception is not null)
            {
                _logger.LogWarning(exception, "Connection disconnected with error.");
            }

            await Clients.All.SendAsync("UpdateUserList", OnlineUsers.Keys.ToArray());
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinPrivateChat(string targetUsername)
        {
            var initiatorUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(initiatorUsername))
            {
                _logger.LogWarning("OnConnectedAsync: Missing username, disconnecting.");
                Context.Abort();
                return;
            }

            var target = (targetUsername ?? string.Empty).Trim();

            if (string.Equals(initiatorUsername, target, StringComparison.OrdinalIgnoreCase)) return;

            OnlineUsers.TryGetValue(target, out var targetConnections);
            if (targetConnections is null || targetConnections.IsEmpty) return;

            var members = new[] { initiatorUsername, target }
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var groupName = $"{members[0]}-{members[1]}";

            if (OnlineUsers.TryGetValue(initiatorUsername, out var initiatorConnections))
            {
                foreach (var conn in initiatorConnections.Keys)
                {
                    await Groups.AddToGroupAsync(conn, groupName);
                }
            }

            foreach (var conn in targetConnections.Keys)
            {
                await Groups.AddToGroupAsync(conn, groupName);
            }

            var groupKey = GroupKeys.GetOrAdd(groupName, _ => Convert.ToBase64String(_aes.GenerateRandomKey()));

            if (initiatorConnections is not null)
            {
                foreach (var conn in initiatorConnections.Keys)
                    await Clients.Client(conn).SendAsync("PrivateChatStarted", groupName, target, groupKey);
            }
            foreach (var conn in targetConnections.Keys)
            {
                await Clients.Client(conn).SendAsync("PrivateChatStarted", groupName, initiatorUsername, groupKey);
            }

            _logger.LogInformation($"Users {initiatorUsername} and {target} added to group {groupName}");
        }

        public async Task SendPrivateMessage(string groupName, string message)
        {
            var username = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("OnConnectedAsync: Missing username, disconnecting.");
                Context.Abort();
                return;
            }

            var chatMessage = ChatMessage.Create(username, message);
            await _chatRepo.SaveMessageAsync(chatMessage);
            _logger.LogInformation($"Message saved, username: {username}, messageId = {chatMessage.Id}, group name: {groupName}");

            await Clients.Group(groupName).SendAsync("ReceivePrivateMessage", groupName, username, message);
            _logger.LogInformation($"Username: {username} sent a message to group :{groupName}");
        }

        public static bool IsUserOnline(string username)
        {
            return OnlineUsers.ContainsKey(username);
        }

    }
}
