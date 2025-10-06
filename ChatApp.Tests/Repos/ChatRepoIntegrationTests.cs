using ChatApp.Core.Models;
using ChatApp.Data.Contexts;
using ChatApp.Data.Repos;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Tests.Repos
{
    public class ChatRepoIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ChatDbContext _context;
        private readonly ChatRepo _repo;

        public ChatRepoIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ChatDbContext(options);
            _context.Database.EnsureCreated();

            _repo = new ChatRepo(_context);
        }

        [Fact]
        public async Task SaveAndGetRecentMessages()
        {
            var m1 = ChatMessage.Create("Dennis", "king");
            var m2 = ChatMessage.Create("Bircan", "du");

            await _repo.SaveMessageAsync(m1);
            await _repo.SaveMessageAsync(m2);

            var recent = await _repo.GetRecentMessagesAsync(10);

            Assert.Equal(2, recent.Count);
            Assert.Equal("Dennis", recent.First().Username);
            Assert.Equal("king", recent.First().Message);
            Assert.Equal("Bircan", recent.Last().Username);

        }

        public void Dispose()
        {
            _context?.Dispose();
            _connection?.Dispose();
        }
    }
}
