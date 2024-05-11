﻿using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Net;

namespace Codex.Controllers
{
    public class ReadingChallengesController : Controller
    {
        private readonly ApplicationDbContext database;

        private readonly UserManager<ApplicationUser> userManager;

        private readonly RoleManager<IdentityRole> roleManager;

        public ReadingChallengesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> _userManager,
            RoleManager<IdentityRole> _roleManager
            )
        {
            database = context;

            userManager = _userManager;

            roleManager = _roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // display the form for adding a new reading challange
        public IActionResult New(string userId)
        {

            var user = getUserById(userId);

            // set user id 
            var newReadingChallenge = new ReadingChallenge
            {
                UserId = user.Id,
            };

            setChallangeDates(ref newReadingChallenge);

            // check to see if user hasnt joined the reading challenge yet 
            if(!user.HasJoinedChallenge(newReadingChallenge))
            {
                ViewBag.UserName = user.Name; 
                return View(newReadingChallenge); 
            }
            else
            {
                TempData["message"] = "You have already joined the reading challenge this year!";
                return RedirectToAction("Profile", "Users", new { id = userId });
            }
           
        }

        [HttpPost]
        public IActionResult New(string userId, ReadingChallenge newReadingChallenge)
        {
            newReadingChallenge.BooksRead = new List<Book>();

            try
            {
                if(ModelState.IsValid)
                {
                    // add challange to database 
                    database.Add(newReadingChallenge);
                    database.SaveChanges();

                    TempData["message"] = "You've joined the reading challenge!";

                    return RedirectToAction("Profile", "Users", new { id = userId });
                }
                else
                {

                    var user = getUserById(userId);
                    ViewBag.UserName = user.Name; 
                    return View(newReadingChallenge); 
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the reading challenge: " + ex.Message);
                
                return View(newReadingChallenge);
            }
        }

        private ApplicationUser getUserById(string id)
        {
            return database.Users.Find(id);
        }

        private void setChallangeDates(ref ReadingChallenge readingChallenge) {
            // get the current date 
            DateTime currentDate = DateTime.Now.Date;
            var currentYear = currentDate.Year;

            // set the start date of the challange to january 1st 
            readingChallenge.StartDate = new DateOnly(currentYear, 1, 1);

            // set the end date of the challange to december 31st 
            readingChallenge.EndDate = new DateOnly(currentYear, 12, 31); 
        }
    }
}