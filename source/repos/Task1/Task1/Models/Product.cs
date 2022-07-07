namespace Task1.Models
{
    public class Product
    {
        [Key]
        [BindNever]
        public int Id { get; set; }
        public Store? Store { get; set; }
        public Brand? Brand { get; set; }
        public User? User { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        [BindNever]
        public string ImagePath { get; set; } = string.Empty;
        public bool deleted { get; set; } = false;
    }
}
