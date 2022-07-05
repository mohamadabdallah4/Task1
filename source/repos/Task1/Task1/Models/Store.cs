namespace Task1.Models
{
    public class Store
    {
        [Key]
        public string Name { get; set; } = string.Empty;
        public Brand? Brand { get; set; }
        [Required]
        public string? BrandName { get; set; } 
        public User? User { get; set; }
        [Required]
        public int? UserId { get; set; }
        public IEnumerable<Product>? Products { get; set; } 
        public IEnumerable<StoreAddress>? Addresses { get; set; }
    }
}
