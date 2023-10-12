using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account
{
    public class ResetPasswordDTO
    {
        [Required]
        public string Token { get; set; }
        [Required]
        [RegularExpression("^\\S+@\\S+\\.\\S+$", ErrorMessage = "Invalid Email")]
        public string Email { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "New password must be at least {2}, and maximum {1} characters")]
        public string NewPassword { get; set; }

    }
} 