using Microsoft.AspNetCore.Identity;

namespace Codex.Models
{
    // the class ApplicationUser extends IdenitityUser
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }

        public virtual ICollection<Review>? Reviews { get; set; }
        public virtual ICollection<Book>? FavoriteBooks { get; set; }
        //public virtual ICollection<Badge>? Badges { get; set; }
    }
}
