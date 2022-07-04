namespace Task1.Models
{
    public class StoreAddress
    {
        public string AddressName { get; set; } = string.Empty;
        public Store? Store { get; set; }
        public string? StoreName { get; set; } 
        public string Status { get; set; } = string.Empty;
    }
}
