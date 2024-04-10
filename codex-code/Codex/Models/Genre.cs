using System.ComponentModel.DataAnnotations;

namespace Codex.Models
{
    public class Genre
    {
        [Key]
        public int GenreId { get; set; }
        [Required(ErrorMessage = "The name of the genre is required!")]
        public int Name {  get; set; }
    }
}
