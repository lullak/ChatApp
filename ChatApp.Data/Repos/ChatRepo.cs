using ChatApp.Core.Interfaces.Repos;
using ChatApp.Core.Models;
using ChatApp.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data.Repos
{
    public class ChatRepo(ChatDbContext context) : IChatRepo
    {
        private readonly ChatDbContext _context = context;

        public async Task SaveMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetRecentMessagesAsync(int limit = 100)
        {
            var recent = await _context.ChatMessages
                .OrderByDescending(m => m.Timestamp)
                .Take(limit)
                .ToListAsync();

            recent.Reverse();
            return recent;
        }
    }
}
