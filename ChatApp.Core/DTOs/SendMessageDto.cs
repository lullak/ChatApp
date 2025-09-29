using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.DTOs
{
    public class SendMessageDto
    {
        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;
    }
}
