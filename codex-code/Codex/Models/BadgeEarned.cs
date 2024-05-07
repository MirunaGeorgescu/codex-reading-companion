using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codex.Models
{
    public class BadgeEarned
    {
        public int? BadgeId { get; set; }
        public virtual ReadingBadge? Badge { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public DateTime? DateEarned { get; set; }
    }
}
