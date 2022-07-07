namespace Task1.Models
{
    public class StoreAddress
    {
        public int Id { get; set; }
        [Required]
        public string AddressName { get; set; } = string.Empty;
        public Store? Store { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
