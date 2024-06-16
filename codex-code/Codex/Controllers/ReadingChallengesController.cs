using Codex.Data;
using Codex.Migrations;
using Codex.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        [Authorize(Roles = "Editor,Admin,User")]
        public IActionResult Show(int id)
        {
            var challenge = getReadingChallengeById(id);
            var user = challenge.User;

            var progress = readingChallengeProgress(user);

            ViewBag.ChallengeProgress = progress;

            return View(challenge);
        }

        // display the form for adding a new reading challange
        [Authorize(Roles = "Editor,Admin,User")]
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
            if (!user.HasJoinedChallenge(newReadingChallenge))
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

        [Authorize(Roles = "Editor,Admin,User")]
        [HttpPost]
        public IActionResult New(string userId, ReadingChallenge newReadingChallenge)
        {
            newReadingChallenge.BooksRead = new List<Book>();

            try
            {
                if (ModelState.IsValid)
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

        [Authorize(Roles = "Editor,Admin,User")]
        public IActionResult Edit(int id)
        {
            var readingChallenge = getReadingChallengeById(id);
            return View(readingChallenge);
        }

        [Authorize(Roles = "Editor,Admin,User")]
        [HttpPost]
        public IActionResult Edit(int id, ReadingChallenge updatedReadingChallenge)
        {
            ReadingChallenge existingReadingChallenge = getReadingChallengeById(id);

            try
            {
                if (ModelState.IsValid)
                {
                    existingReadingChallenge.TargetNumber = updatedReadingChallenge.TargetNumber;
                    database.SaveChanges();
                    return RedirectToAction("Show", "ReadingChallenges", new { id = id });

                }
                else
                {
                    return View(updatedReadingChallenge);
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the reading challenge: " + ex.Message);

                return View(updatedReadingChallenge);
            }
        }

        [Authorize(Roles = "Editor,Admin,User")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var readingChallenge = getReadingChallengeById(id);

            // get the user id for redirecting 
            var userId = readingChallenge.UserId;

            database.ReadingChallenges.Remove(readingChallenge);
            database.SaveChanges();

            TempData["message"] = "You've deleted this year's reading challenge!";

            return RedirectToAction("Profile", "Users", new { id = userId });
        }

        [Authorize(Roles = "Editor,Admin,User")]
        public IActionResult YearlyRecap(string userId)
        {
            var user = getUserById(userId);

            var readingChallange = database.ReadingChallenges
                                    .Include(rc => rc.BooksRead)
                                    .Include(rc => rc.User)
                                    .FirstOrDefault(rc => rc.UserId == userId && rc.EndDate.Year == DateTime.Now.Year);

            if (readingChallange == null)
            {
                return View(null);
            }

            // get some statisitics
            var averageRating = GetAverageRating(readingChallange);
            var totalBooksRead = readingChallange.BooksRead.Count;
            var longestBookRead = GetLongestBook(readingChallange);
            var shortestBookRead = GetShortestBook(readingChallange);
            var totalNumberOfPages = GetTotalNumberOfPages(readingChallange);
            var averageNumberOfPagesRead = totalBooksRead == 0 ? 0 : totalNumberOfPages / totalBooksRead;
            var highestRatedBookRead = GetHighestRatedBookRead(readingChallange);

            // add them to viewbag for easy access 
            ViewBag.AverageRating = averageRating;
            ViewBag.TotalBooksRead = totalBooksRead;
            ViewBag.LongestBookRead = longestBookRead;
            ViewBag.ShortestBookRead = shortestBookRead;
            ViewBag.TotalNumberOfPages = totalNumberOfPages;
            ViewBag.AverageNumberOfPagesRead = averageNumberOfPagesRead;
            ViewBag.HighestRatedBookRead = highestRatedBookRead;


            return View(readingChallange);
        }

        private ApplicationUser getUserById(string id)
        {
            return database.Users.Find(id);
        }

        private void setChallangeDates(ref ReadingChallenge readingChallenge)
        {
            // get the current date 
            DateTime currentDate = DateTime.Now.Date;
            var currentYear = currentDate.Year;

            // set the start date of the challange to january 1st 
            readingChallenge.StartDate = new DateOnly(currentYear, 1, 1);

            // set the end date of the challange to december 31st 
            readingChallenge.EndDate = new DateOnly(currentYear, 12, 31);
        }

        private ReadingChallenge getReadingChallengeById(int id)
        {
            return database.ReadingChallenges
                .Include(rc => rc.User)
                .Include(rc => rc.BooksRead)
                .FirstOrDefault(rc => rc.ReadingChallengeId == id);
        }

        private double? readingChallengeProgress(ApplicationUser user)
        {
            if (user.HasJoinedChallenge(new ReadingChallenge { StartDate = new DateOnly(DateTime.Now.Year, 1, 1), UserId = user.Id }))
            {
                // find the reading challenge 
                var readingChallenge = database.ReadingChallenges
                                            .FirstOrDefault(rc => rc.UserId == user.Id
                                                                && rc.StartDate.Year == DateTime.Now.Year);

                // find how many books the user wants to read 
                var targetBooks = readingChallenge.TargetNumber;

                // find how many books the user has read
                var booksRead = 0;
                if (readingChallenge.BooksRead != null)
                {
                    booksRead = readingChallenge.BooksRead.Count;
                }

                // calculate progress procentage
                return Math.Round((double)booksRead / targetBooks * 100, 2);
            }

            // if the user hasn't joined the reading challenge then return null
            return null;
        }

        private double GetAverageRating(ReadingChallenge readingChallenge)
        {
            if (readingChallenge.BooksRead == null || readingChallenge.BooksRead.Count == 0)
                return 0;

            var ratings = readingChallenge.BooksRead.Sum(book => (int)book.Rating);
            return (double)ratings / readingChallenge.BooksRead.Count;
        }

        private Book GetLongestBook(ReadingChallenge readingChallenge)
        {
            return readingChallenge.BooksRead?.OrderByDescending(book => book.NumberOfPages).FirstOrDefault();
        }

        private Book GetShortestBook(ReadingChallenge readingChallenge)
        {
            return readingChallenge.BooksRead?.OrderBy(book => book.NumberOfPages).FirstOrDefault();
        }

        private int GetTotalNumberOfPages(ReadingChallenge readingChallenge)
        {
            return readingChallenge.BooksRead?.Sum(book => book.NumberOfPages) ?? 0;
        }

        private Book GetHighestRatedBookRead(ReadingChallenge readingChallenge)
        {
            return readingChallenge.BooksRead?.OrderByDescending(book => book.Rating).FirstOrDefault();
        }

    }
}

