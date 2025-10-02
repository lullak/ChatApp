using ChatApp.Core.Models;

namespace ChatApp.Core.Interfaces.Repos
{
    public interface IChatRepo
    {
        public Task SaveMessageAsync(ChatMessage message);
        public Task<List<ChatMessage>> GetRecentMessagesAsync(int limit = 100);
    }
}
