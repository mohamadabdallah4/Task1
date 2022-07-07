namespace Task1.Models
{
    public class NewProductRequest
    {
        [Required]
        public string name { get; set; } = string.Empty;
        [Required]
        public string brandName { get; set; } = string.Empty;
        [Required]
        public decimal price { get; set; }
    }
}
