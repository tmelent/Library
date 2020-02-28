using Library.Models;
using Library.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Library.Data
{
    public class LibraryContext : DbContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public LibraryContext()
        {
            Database.EnsureCreated();          
        }
        
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder();            
            builder.SetBasePath(Directory.GetCurrentDirectory());            
            builder.AddJsonFile("appsettings.json");            
            var config = builder.Build();           
            string connectionString = config.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
