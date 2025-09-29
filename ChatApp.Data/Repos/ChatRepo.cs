using ChatApp.Core.Interfaces.Repos;
using ChatApp.Core.Models;
using ChatApp.Data.Contexts;

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
    }
}
