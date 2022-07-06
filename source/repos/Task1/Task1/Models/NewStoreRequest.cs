namespace Task1.Models
{
    public class NewStoreRequest
    {
        [Required]
        public string storeName { get; set; } = string.Empty;
        [Required]
        public string brandName { get; set; } = string.Empty;
        [Required]
        public string initialStoreAddress { get; set; } = string.Empty;
        [Required]
        public string initialAddressStatus { get; set; } = string.Empty;
    }
}
