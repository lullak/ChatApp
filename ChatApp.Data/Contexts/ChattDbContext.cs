using ChatApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data.Contexts
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
        }

        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
