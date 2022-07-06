namespace Task1.Models
{
    public class NewPasswordRequest
    {
        [Required]
        public string OldPassword { get; set; } = string.Empty;
        [Required]
        public string NewPassword { get; set; } = string.Empty;
        [Required, Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
    }
}
