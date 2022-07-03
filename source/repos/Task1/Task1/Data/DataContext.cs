using Microsoft.EntityFrameworkCore;
using Task1.Models;

namespace Task1.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UnconfirmedUser> UnconfirmedUsers { get; set; }
        public DbSet<UserPassword> UserPasswords { get; set; }
    }
}
