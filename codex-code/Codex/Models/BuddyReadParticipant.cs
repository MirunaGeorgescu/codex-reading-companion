using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codex.Models
{
    public class BuddyReadParticipant
    {
        public int? BuddyReadId { get; set; }
        public virtual BuddyRead? BuddyRead { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
