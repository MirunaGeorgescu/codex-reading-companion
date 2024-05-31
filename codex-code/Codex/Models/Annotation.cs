using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codex.Models
{
    public class Annotation
    {
        [Key]
        public int AnnotationId { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public int? BuddyReadId { get; set; }
        public virtual BuddyRead? BuddyRead { get; set; }

        [Required(ErrorMessage = "The comment is required!")]
        public string Comment { get; set; }

        [Required(ErrorMessage = "The page is required!")]
        public int Page {  get; set; }

        public DateTime TimeStamp { get; set; }


    }
}
