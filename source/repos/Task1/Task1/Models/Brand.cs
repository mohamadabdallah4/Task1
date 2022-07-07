namespace Task1.Models
{
    public class Brand
    {
        [Key]
        public string Name { get; set; } = string.Empty;
        public User? User { get; set; } 
    }
}
