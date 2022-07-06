namespace Task1.Models
{
    public class StoreAddress
    {
        [Key]
        public int StoreAddressId { get; set; }
        [Required]
        public string AddressName { get; set; } = string.Empty;
        [JsonIgnore]
        public Store? Store { get; set; }
        [ForeignKey("Store")]
        public string? StoreName { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
