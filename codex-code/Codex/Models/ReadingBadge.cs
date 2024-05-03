using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codex.Models
{
    public class ReadingBadge
    {
        [Key]
        public int BadgeId { get; set; }

        [Required(ErrorMessage = "The badge name is required!")]
        public string BadgeName { get; set; }

        [Required(ErrorMessage = "The badge description is required!")]
        public string BadgeDescription { get; set; }

        [Required(ErrorMessage = "The badge image is required!")]
        public string BadgeImage {  get; set; }

    }
}
