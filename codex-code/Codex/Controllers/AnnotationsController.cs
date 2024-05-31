using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Codex.Controllers
{
    public class AnnotationsController : Controller
    {
        private readonly ApplicationDbContext database;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AnnotationsController(ApplicationDbContext context,
            UserManager<ApplicationUser> _userManager,
            RoleManager<IdentityRole> _roleManager)
        {
            database = context;
            userManager = _userManager;
            roleManager = _roleManager;
        }

        public IActionResult New(int buddyReadId)
        {
            // get the current users Id, the one leaving the annotation
            var userId = userManager.GetUserId(User); 

            var annotation = new Annotation();
            annotation.UserId = userId;
            annotation.BuddyReadId = buddyReadId;

            return View(annotation);
        }

        [HttpPost]
        public IActionResult New(int buddyReadId, Annotation annotation)
        {
            if(ModelState.IsValid)
            {
                var userId = annotation.UserId;
                var user = getUserById(userId);

                var buddyRead = database.BuddyReads
                                    .Include(br => br.Book)
                                    .FirstOrDefault(br => br.BuddyReadId == buddyReadId);
                var book = database.Books
                                .FirstOrDefault(b => b.BookId == buddyRead.BookId);
                int bookId = book.BookId; 

                var currentlyReading = database.Shelves
                                        .Include(s => s.BooksOnShelves)
                                        .FirstOrDefault(s => s.UserId == user.Id && s.Name == "Currently reading");

                var usersBookOnShelf = database.BooksOnShelves
                                        .FirstOrDefault(bos => bos.BookId == bookId && bos.ShelfId == currentlyReading.ShelfId);

                var usersCurrentPage = usersBookOnShelf.CurrentPage;

                if (usersCurrentPage < annotation.Page)
                {
                    return View(annotation);
                }

                annotation.TimeStamp = DateTime.Now;

                database.Add(annotation);
                database.SaveChanges();

                return RedirectToAction("Show", "BuddyReads", new { id = buddyReadId });
            }
            else
            {
                return View(annotation);
            }
        }

        private ApplicationUser getUserById(string id)
        {
            return database.Users
                 .Include(u => u.Followers)
                 .Include(u => u.Following)
                .FirstOrDefault(u => u.Id == id);
        }

    }
}
