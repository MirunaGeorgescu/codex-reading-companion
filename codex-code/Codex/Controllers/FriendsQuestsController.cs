using AngleSharp.Text;
using Codex.Data;
using Codex.Models;
using Codex.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Diagnostics.Metrics;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Ganss.Xss;


namespace Codex.Controllers
{
    public class FriendsQuestsController : Controller
    {
        private readonly ApplicationDbContext database;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public FriendsQuestsController(ApplicationDbContext context,
            UserManager<ApplicationUser> _userManager,
            RoleManager<IdentityRole> _roleManager)
        {
            database = context;
            userManager = _userManager;
            roleManager = _roleManager;
        }

        public IActionResult New()
        {
            var currentUserId = userManager.GetUserId(User); 
            var currentUser = database.ApplicationUsers
                                .Include(u => u.Followers)
                                .Include(u => u.Following)
                                .FirstOrDefault(u => u.Id == currentUserId);

            var friendsQuest = new FriendsQuest
            {
                UserId1 = currentUserId
            }; 

            // find the users friends for the friends quest
            var friendsList = getUsersFriends(currentUserId);

            // filter through the friends to see which ones are not already in a friends quest 
            var availableFriends = friendsList.Where(f => !database.FriendsQuests
                                               .Any(fq => (fq.UserId1 == f.Id || fq.UserId2 == f.Id) && fq.EndDate > DateTime.Now))
                                     .ToList();

            // trun it into a select list 
            ViewBag.AvailableFriends = new SelectList(availableFriends, "Id", "Name");
            return View(friendsQuest);  

        }

        [HttpPost]
        public IActionResult New(FriendsQuest friendsQuest)
        {
            if (ModelState.IsValid)
            {
                friendsQuest.StartDate = DateTime.Now;
                friendsQuest.EndDate = DateTime.Now.AddDays(7);
                friendsQuest.PagesRead = 0;
                friendsQuest.TargetPages = generateRandomGoal();

                database.FriendsQuests.Add(friendsQuest);
                database.SaveChanges();

                return RedirectToAction("Profile", "Users", new {id = friendsQuest.UserId1});
            }
            else
            {
                var currentUserId = userManager.GetUserId(User);

                var friendsList = getUsersFriends(currentUserId);
                var availableFriends = friendsList.Where(f => !database.FriendsQuests
                                                .Any(fq => (fq.UserId1 == f.Id || fq.UserId2 == f.Id) && fq.EndDate > DateTime.Now))
                                      .ToList();

                ViewBag.AvailableFriends = new SelectList(availableFriends, "Id", "Name");

                return View(friendsQuest);
            }
        }


        private List<ApplicationUser> getUsersFriends(string userId)
        {
            var user = database.Users
                        .Include(u => u.Followers)
                        .Include(u => u.Following)
                        .FirstOrDefault(u => u.Id == userId);

            var friends = new List<ApplicationUser>();

            foreach (var follower in user.Followers)
            {
                if (user.Following.Contains(follower))
                    friends.Add(follower);
            }

            return friends;
        }

        private int generateRandomGoal()
        {
            Random random = new Random();
            return random.Next(220, 750);
        }
    }
}
