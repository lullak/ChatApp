using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        [MinLength(3, ErrorMessage = "Username must be atleast 3 characters")]
        [MaxLength(32, ErrorMessage = "Username max length is 32 characters")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username can only contain letters and numbers. Max length is 32.")]
        public string Username { get; set; } = string.Empty;
    }
}
