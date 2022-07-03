namespace Task1.Models
{
    public class NewPasswordRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
