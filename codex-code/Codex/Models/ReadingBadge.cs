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
        public string Name { get; set; }

        [Required(ErrorMessage = "The badge description is required!")]
        public string Description { get; set; }

        [Required(ErrorMessage = "The badge image is required!")]
        public string Image {  get; set; }

    }
}
