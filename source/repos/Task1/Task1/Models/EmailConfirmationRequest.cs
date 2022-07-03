namespace Task1.Models
{
    public class EmailConfirmationRequest
    {
        public string ConfirmationCode { get; set; } = string.Empty;
        public string NewEmail { get; set; } = string.Empty;
    }
}
