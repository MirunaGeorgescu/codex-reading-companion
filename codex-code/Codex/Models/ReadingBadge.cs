using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Codex.Enums; 

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


        // CRITERIA FOR AWARDING THE BADGES

        // the type of badge

        [Required(ErrorMessage = "The badge type is required!")]
        public CriteriaType CriteriaType { get; set; }

        // the target number of books

        [Required(ErrorMessage = "The target number of books is required!")]
        public int CriteriaValue { get; set; }

        // some extra info like the name of the author or genre
        public string? TargetName { get; set; }


        // the options for the criteria type drop down menu 
        [NotMapped]
        public IEnumerable<SelectListItem>? CriteriaTypeOptions { get; set; }

        public virtual ICollection<BadgeEarned>? BadgesEarned { get; set; }
    }
}
