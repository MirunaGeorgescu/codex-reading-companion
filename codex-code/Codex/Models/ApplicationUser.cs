﻿using Microsoft.AspNetCore.Identity;
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
        private static readonly string DefaultProfilePictureUrl = "https://media.istockphoto.com/id/1337144146/vector/default-avatar-profile-icon-vector.jpg?s=612x612&w=0&k=20&c=BIbFwuv7FxTWvh5S3vB6bkT0Qv8Vn8N5Ffseq84ClGI=";

        public void UseDefaultProfilePictureUrl()
        {
            ProfilePhoto = DefaultProfilePictureUrl;
        }
    }
}