using Codex.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codex.Models
{
    // the class ApplicationUser extends IdenitityUser
    public class ApplicationUser : IdentityUser
    {
       
        public string? Name { get; set; }
        public string? ProfilePhoto { get; set; }

        public virtual ICollection<Review>? Reviews { get; set; }
        public virtual ICollection<Book>? FavoriteBooks { get; set; }
        public virtual ICollection<BadgeEarned>? BadgesEarned { get; set; }
        public virtual ICollection<Shelf>? Shelves{ get; set; }
        public virtual ICollection<ReadingBadge>? Badges {  get; set; }
        public virtual ICollection<ReadingChallenge>? ReadingChallenges { get; set; }
        public virtual ICollection<ApplicationUser>? Followers { get; set; }
        public virtual ICollection<ApplicationUser>? Following { get; set; }



        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }

        // default profile picture 
        private static readonly string DefaultProfilePictureUrl = "https://media.istockphoto.com/id/1337144146/vector/default-avatar-profile-icon-vector.jpg?s=612x612&w=0&k=20&c=BIbFwuv7FxTWvh5S3vB6bkT0Qv8Vn8N5Ffseq84ClGI=";

        [NotMapped]
        public IEnumerable<SelectListItem>? FavoriteBooksOptions { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? ShelvesOptions { get; set; }

        public void UseDefaultProfilePictureUrl()
        {
            ProfilePhoto = DefaultProfilePictureUrl;
        }

        public bool HasJoinedChallenge(ReadingChallenge readingChallenge)
        {
            return ReadingChallenges != null
                && ReadingChallenges.Any(c => c.StartDate.Year == readingChallenge.StartDate.Year
                                            && c.UserId == readingChallenge.UserId);
        }

    }
}
