using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account
{
    public class ConfirmEmailDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [RegularExpression("^\\S+@\\S+\\.\\S+$", ErrorMessage = "Invalid Email")]
        public string Email { get; set; }
    }
}