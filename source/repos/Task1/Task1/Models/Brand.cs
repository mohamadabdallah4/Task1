namespace Task1.Models
{
    public class Brand
    {
        [Key]
        public string Name { get; set; } = string.Empty;
        [JsonIgnore]
        public User? User { get; set; } 
        public int? UserId { get; set; }
    }
}
