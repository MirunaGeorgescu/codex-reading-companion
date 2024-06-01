using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codex.Models
{
    public class FriendsQuest
    {
        [Key]
        public int QuestId { get; set; }

        public string? UserId1 { get; set; }
        public ApplicationUser? User1 { get; set; }

        public string? UserId2 { get; set; }
        public ApplicationUser? User2 { get; set; }

        public DateTime StartDate {  get; set; }
        public DateTime EndDate { get; set; }

        public int TargetPages { get; set; }
        public int PagesRead { get; set; }
    }
}
