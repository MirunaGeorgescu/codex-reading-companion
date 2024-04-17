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

        [NotMapped]
        public IEnumerable<SelectListItem>? AllShelves { get; set; }
    }
}
