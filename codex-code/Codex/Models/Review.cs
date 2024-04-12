using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Codex.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "The rating is required!")]
        [Range(1, 5, ErrorMessage = "Your rating must be a number between 1 and 5!")]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime Date { get; set; }

        public string? UserId { get; set; }
        public virtual IdentityUser? User { get; set; }

        public int? BookId { get; set; }
        public virtual Book? Book { get; set;}
    }
}
