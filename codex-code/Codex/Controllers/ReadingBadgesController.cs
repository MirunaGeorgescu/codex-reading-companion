using Codex.Data;
using Codex.Enums;
using Codex.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            // populate the options for the reading badge types 
            var newBadgeInfo = new ReadingBadge
            {
                CriteriaTypeOptions = getCriteriaTypeOptions()
            };

            return View(newBadgeInfo);
        }

        // add the new badge to the database
        [HttpPost]
        public IActionResult New(ReadingBadge newReadingBadge)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (isBadgeUnique(newReadingBadge))
                    {
                        // validate the badge type and input
                        if((newReadingBadge.CriteriaType == CriteriaType.BooksRead || newReadingBadge.CriteriaType == CriteriaType.BooksToRead)
                            && (!string.IsNullOrEmpty(newReadingBadge.TargetName)))
                        {
                            ModelState.AddModelError("TargetName", "Target name should not be provided for badge types BooksRead or BooksToRead.");
                            newReadingBadge.CriteriaTypeOptions = getCriteriaTypeOptions(); 
                            return View(newReadingBadge);
                        }

                        // adding the genre to the databse if its unique
                        database.Add(newReadingBadge);
                        database.SaveChanges();

                        TempData["message"] = "The reading badge " + newReadingBadge.Name + " was added to the database!";

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("Name", "The reading badge " + newReadingBadge.Name + " already exists in the database!");
                        newReadingBadge.CriteriaTypeOptions = getCriteriaTypeOptions();
                        return View(newReadingBadge);
                    }
                }
                else
                {
                    newReadingBadge.CriteriaTypeOptions = getCriteriaTypeOptions();
                    return View(newReadingBadge);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the reading badge: " + ex.Message);
                newReadingBadge.CriteriaTypeOptions = getCriteriaTypeOptions();
                return View(newReadingBadge);
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            // find the reading badge in the database 
            ReadingBadge badge = getBadgeById(id);

            // removing the genre from the database
            database.ReadingBadges.Remove(badge);

            TempData["message"] = "The genre " + badge.Name + " was removed from the database!";

            // saving the changes made 
            database.SaveChanges();
            return RedirectToAction("Index");

        }

        public IActionResult Edit(int id)
        {
            // find badge by id
            var readingBadge = getBadgeById(id);

            // send the badge to the view
            return View(readingBadge);
        }

        [HttpPost]
        public IActionResult Edit(int id, ReadingBadge newReadingBadge)
        {
            // finding the original badge in the database
            var oldReadingBadge = getBadgeById(id);

            try
            {
                if (ModelState.IsValid)
                {
                    // if the new badge is valid then we map the attributes to the old badge and save the changes 
                    mapBadgeAttributes(ref oldReadingBadge, newReadingBadge);
                    database.SaveChanges();

                    TempData["message"] = "The badge " + oldReadingBadge.Name + " was succesfully edited!";

                    // back to the index page so we can see the changes 
                    return RedirectToAction("Index");
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
        private bool isBadgeUnique(ReadingBadge readingBadge)
        {
            // see if there is a badge with the same name in teh database
            var badge = database.ReadingBadges
                .FirstOrDefault(rb => rb.Name == readingBadge.Name);


            if (badge == null)
            {
                // try to find a badge with the same description
                badge = database.ReadingBadges
                    .FirstOrDefault(rb => rb.Description == readingBadge.Description);

                if (badge == null)
                {
                    // try to find a badge with the same image
                    badge = database.ReadingBadges
                        .FirstOrDefault(rb => rb.Image == readingBadge.Image);

                    if (badge == null)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }

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
        private ReadingBadge getBadgeById(int id)
        {
            return database.ReadingBadges
                .FirstOrDefault(rb => rb.BadgeId == id);
        }

        private void mapBadgeAttributes(ref ReadingBadge destination, ReadingBadge source)
        {
            destination.Name = source.Name;
            destination.Description = source.Description;
            destination.Image = source.Image;
        }

        private IEnumerable<SelectListItem> getCriteriaTypeOptions()
        {
            // getting the criteria types defined in the enum in order to display them in a drop down menu
            var criteriaTypeOptions = Enum.GetValues(typeof(CriteriaType))
                                         .Cast<CriteriaType>()     
                                         .Select(criteriaType => new SelectListItem
                                         {
                                             Value = criteriaType.ToString(),
                                             Text = criteriaType.ToString()
                                         })
                                         .ToList();

            return criteriaTypeOptions;
        }
    }
}
