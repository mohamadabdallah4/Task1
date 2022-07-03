﻿namespace Task1.Models
{
    public class UnconfirmedUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmationCode { get; set; } = string.Empty;
    }
}
