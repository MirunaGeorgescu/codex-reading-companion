using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;

namespace Codex.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public float Rating { get; set; }

        public string Genre { get; set; }

       // public ICollection<string>? Tags { get; set; }

        public string Synopsis { get; set; }

        public DateOnly PublicationDate { get; set; }

        // CoverImage stores the path of the image 
        public string CoverImage { get; set; }
    }
}