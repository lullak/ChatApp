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
    }
}
