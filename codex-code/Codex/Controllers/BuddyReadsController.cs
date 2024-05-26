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


                var br = database.BuddyReads.FirstOrDefault (br => br.BookId == newBuddyRead.BookId
                && br.StartDate == newBuddyRead.StartDate
                && br.ParticipantIds.Last() == currentUserId);

                foreach(var participantId in newBuddyRead.ParticipantIds)
                {
                    var newBuddyReadParticipant = new BuddyReadParticipant {
                        BuddyReadId = br.BuddyReadId, 
                        UserId = participantId
                    };

                    database.BuddyReadsParticipants.Add(newBuddyReadParticipant);
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
    }
}