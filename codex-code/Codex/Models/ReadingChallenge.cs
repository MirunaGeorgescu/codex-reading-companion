using System.ComponentModel.DataAnnotations;

namespace Codex.Models
{
    public class ReadingChallenge
    {
        [Key]
        public int ReadingChallengeId { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }


        [Required(ErrorMessage ="The target number of books is required!")]
        public int TargetNumber { get; set; }


        public virtual ICollection<Book> BooksRead { get; set; }    
    }
}
