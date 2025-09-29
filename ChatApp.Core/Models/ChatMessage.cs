using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        private ChatMessage() { }

        public static ChatMessage Create(string username, string message)
        {
            var chatMessage = new ChatMessage
            {
                Username = username.Trim(),
                Message = message.Trim(),
                Timestamp = DateTime.UtcNow
            };

            return chatMessage;
        }
    }
}
