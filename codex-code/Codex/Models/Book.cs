using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;

namespace Codex.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required(ErrorMessage = "The title is required!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "The author is required!")]
        public string Author { get; set; }

        public float? Rating { get; set; }

        [Required(ErrorMessage = "The genre is required!")]
        public int? GenreId { get; set; }
        public virtual Genre Genre { get; set; }

        // public ICollection<string>? Tags { get; set; }

        [Required(ErrorMessage = "The synopsis is required!")]
        public string Synopsis { get; set; }

        [Required(ErrorMessage = "The publication date is required!")]
        public DateOnly PublicationDate { get; set; }

        // CoverImage stores the path of the image 
        [Required(ErrorMessage = "The cover image is required!")]
        public string CoverImage { get; set; }
    }
}