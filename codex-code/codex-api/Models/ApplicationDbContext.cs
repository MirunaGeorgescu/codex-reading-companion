using Microsoft.EntityFrameworkCore;

namespace codex_api.Models
{
    // The ApplivationDbContext is responsible for connecting the .NET application to the database
    // DbContext is a class provided by EntityFramework and it represents a session with the database
    // and allows me to query and save instances of entity classes
    public class ApplicationDbContext : DbContext 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
    }
}
