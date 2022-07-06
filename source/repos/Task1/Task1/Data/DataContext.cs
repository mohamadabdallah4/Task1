using Microsoft.EntityFrameworkCore;
using Task1.Models;

namespace Task1.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<UserPassword> UserPasswords { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<StoreAddress> StoreAddresses { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}
