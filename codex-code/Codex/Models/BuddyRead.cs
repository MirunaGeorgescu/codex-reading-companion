using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codex.Models
{
    public class BuddyRead
    {
        [Key]
        public int BuddyReadId { get; set; }

        public int? BookId { get; set; }
        public virtual Book? Book { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual ICollection<Annotation> Annotations { get; set; }


    }
}
