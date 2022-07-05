namespace Task1.Models
{
    public class Product
    {
        [Key]
        [BindNever]
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public Store? Store { get; set; }
        public string? StoreName { get; set; }
        [JsonIgnore]
        public Brand? Brand { get; set; }
        public string? BrandName { get; set; }
        [JsonIgnore]
        public User? User { get; set; }
        public int? UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        [BindNever]
        [JsonIgnore]
        public string ImagePath { get; set; } = string.Empty;
        public bool deleted { get; set; } = false;
    }
}
