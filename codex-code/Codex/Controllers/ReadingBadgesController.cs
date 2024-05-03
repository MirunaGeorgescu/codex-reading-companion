using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Codex.Controllers
{
    public class ReadingBadgesController : Controller
    {
        private readonly ApplicationDbContext database;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public ReadingBadgesController(ApplicationDbContext context,
            UserManager<ApplicationUser> _userManager,
            RoleManager<IdentityRole> _roleManager)
        {
            database = context;
            userManager = _userManager;
            roleManager = _roleManager;
        }

        public IActionResult Index()
        {
            // get all the badges form the database
            var allBadges = getAllBadges();

            // send them to the view to display 
            return View(allBadges);
        }

        // display form for adding a new reading badge 
        public IActionResult New()
        {
            return View(); 
        }

        // add the new badge to the database
        [HttpPost]
        public IActionResult New(ReadingBadge newReadingBadge) 
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (isBageUnique(newReadingBadge))
                    {
                        // adding the genre to the databse if its unique
                        database.Add(newReadingBadge);
                        database.SaveChanges();

                        TempData["message"] = "The reading badge " + newReadingBadge.Name + " was added to the database!";

                        return Redirect("");
                    }
                    else
                    {
                        ModelState.AddModelError("Name", "The reading badge " + newReadingBadge.Name + " already exists in the database!");
                        return View(newReadingBadge);
                    }
                }
                else
                {
                    return View(newReadingBadge);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the reading badge: " + ex.Message);
                return View(newReadingBadge);
            }
        }

        private bool isBageUnique(ReadingBadge readingBadge)
        {
            // see if i can find a badge with the same name in teh database
            var readingBadgeWithSameName = getReadingBadgeByName(readingBadge.Name);

            // if theres no badge with the same name return true, else false
            return readingBadgeWithSameName == null; 
        }

        private ReadingBadge getReadingBadgeByName(string name)
        {
            return database.ReadingBadges
                .FirstOrDefault(rb => rb.Name == name); 
        }

        private IEnumerable<ReadingBadge> getAllBadges()
        {
            return database.ReadingBadges.ToList(); 
        }
    }

   
}
