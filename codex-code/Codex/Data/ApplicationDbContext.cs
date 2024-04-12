using Codex.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Codex.Data
{
    // The ApplivationDbContext is responsible for connecting the .NET application to the database
    // DbContext is a class provided by EntityFramework and it represents a session with the database
    // and allows me to query and save instances of entity classes
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set;  }
        public DbSet<Review> Reviews { get; set; }

    }
}
