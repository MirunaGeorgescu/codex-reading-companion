using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codex.Models
{
    public class Shelf
    {
        [Key]
        public int ShelfId { get; set; }

        [Required(ErrorMessage =  "A shelf must have a name!")]
        public string Name { get; set; }

        // the shelf belongs to a user
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // the books that are on the shelf
        public ICollection<BookOnShelf>? BooksOnShelves { get; set; }
    }
}
