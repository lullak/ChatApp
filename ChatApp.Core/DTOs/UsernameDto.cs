using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.DTOs
{
    public class UsernameDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(32)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username can only contain letters and numbers. Max length is 32.")]
        public string Username { get; set; } = string.Empty;
    }
}
