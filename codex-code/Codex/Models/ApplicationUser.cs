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
        //public virtual ICollection<Badge>? Badges { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }

        // default profile picture 
        public static readonly string DefaultProfilePictureUrl = "https://www.bing.com/images/create/default-user-picture-silhouette-inspired-by-the-go/2-661e762cab954dae8b4df260a4e1e659?id=SFyLs9JBvrd25LIRiYQVBQ%3d%3d&view=detailv2&idpp=genimg&idpclose=1&thId=OIGBCE3.bmg4hKtixyLjj9JPnvtb&FORM=SYDBIC";

        // method for setting the default profile picture
        public void SetDefaultProfilePicture()
        {
            if (string.IsNullOrEmpty(ProfilePhoto))
            {
                ProfilePhoto = DefaultProfilePictureUrl;
            }
        }


    }
}
