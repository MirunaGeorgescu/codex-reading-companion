using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codex.Models
{
    public class BookOnShelf
    {
        public int? BookId { get; set; }
        public virtual Book? Book {  get; set; } 

        public int? ShelfId {  get; set; }
        public virtual Shelf? Shelf { get; set; }

        // for the books the user is currently reading 
        public int? CurrentPage { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? AllShelves { get; set; }
    }
}
