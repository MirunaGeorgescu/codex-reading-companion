using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Codex.Controllers
{
    public class BuddyReadsController : Controller
    {
        private readonly ApplicationDbContext database;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public BuddyReadsController(ApplicationDbContext context,
            UserManager<ApplicationUser> _userManager,
            RoleManager<IdentityRole> _roleManager)
        {
            database = context;
            userManager = _userManager;
            roleManager = _roleManager;
        }

        public IActionResult New()
        {
            // get all the books 
            ViewBag.Books = getAllBooks();

            // get the current user 
            var currentUserId = userManager.GetUserId(User); 

            // get the users friends 
            ViewBag.Friends = getUsersFriends(currentUserId);

            return View();
        }

        [HttpPost]
        public IActionResult New(BuddyRead buddyRead)
        {
            if(ModelState.IsValid)
            {
                // get current user and add to the list of participants  
                var currentUserId = userManager.GetUserId(User);
                buddyRead.ParticipantIds.Add(currentUserId); 

                var newBuddyRead = new BuddyRead
                {
                    BookId = buddyRead.BookId,
                    StartDate = DateTime.Now,
                    EndDate = null, 
                    ParticipantIds = buddyRead.ParticipantIds
                }; 

                database.BuddyReads.Add(newBuddyRead);
                database.SaveChanges();


                var br = database.BuddyReads
                    .Include(b => b.Book)
                    .FirstOrDefault (b => b.BookId == newBuddyRead.BookId
                && b.StartDate == newBuddyRead.StartDate
                && b.ParticipantIds.Last() == currentUserId);

                foreach(var participantId in newBuddyRead.ParticipantIds)
                {
                    var newBuddyReadParticipant = new BuddyReadParticipant {
                        BuddyReadId = br.BuddyReadId, 
                        UserId = participantId
                    };

                    database.BuddyReadsParticipants.Add(newBuddyReadParticipant);

                    // add the book to their currently reading shelf
                    moveBookToCurrentlyReadingShelf((int)br.BookId, participantId);

                }

                database.SaveChanges();

              
                return RedirectToAction("Profile", "Users", new { id = currentUserId });
            }
            else
            {
                // get all the books 
                ViewBag.Books = getAllBooks();

                // get the current user 
                var currentUserId = userManager.GetUserId(User);

                // get the users friends 
                ViewBag.Friends = getUsersFriends(currentUserId);

                return View(buddyRead);
            }
        }

        private List<Book> getAllBooks()
        {
            List<Book> allBooks = database.Books.ToList();
            return allBooks;
        }

        private List<ApplicationUser> getUsersFriends(string userId) {
            var user = database.Users
                        .Include(u => u.Followers)
                        .Include(u => u.Following)
                        .FirstOrDefault(u => u.Id  == userId);

            var friends = new List<ApplicationUser>();

            foreach(var follower in user.Followers)
            {
                if(user.Following.Contains(follower))
                    friends.Add(follower);
            }

            return friends; 
        }

        private void moveBookToCurrentlyReadingShelf(int bookId, string userId)
        {
            var currentlyReadingShelf = database.Shelves
                                            .Include(s => s.BooksOnShelves)
                                            .FirstOrDefault(s => s.UserId == userId && s.Name == "Currently reading");

            // if the user has shelved the book before, delete it from that shelf
            var bookOnShelf = database.BooksOnShelves
                                .Include(bos => bos.Shelf)
                                .FirstOrDefault(bos => bos.BookId == bookId && bos.Shelf.UserId == userId); 
            if(bookOnShelf != null)
                database.BooksOnShelves.Remove(bookOnShelf);

            // add the book to the currently reading list of the participant 
            var buddyReadOnShelf = new BookOnShelf
            { 
                BookId = bookId,
                ShelfId = currentlyReadingShelf.ShelfId, 
                CurrentPage = 0
            };

            database.BooksOnShelves.Add(buddyReadOnShelf);
            database.SaveChanges();
        }
    }
}