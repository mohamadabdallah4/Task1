namespace Task1.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [JsonIgnore]
        public string Password { get; set; } = string.Empty;
        [JsonIgnore]
        public string ImagePath { get; set; } = string.Empty;
        [JsonIgnore]
        public string? ConfirmationCode { get; set; } = null;
        [JsonIgnore]
        public bool Confirmed { get; set; } = true;

        public string LastPasswordChange { get; set; } = String.Empty;
    }
}
 