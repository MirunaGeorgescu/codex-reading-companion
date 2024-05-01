using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace Codex.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext database;

        private readonly UserManager<ApplicationUser> userManager;

        private readonly RoleManager<IdentityRole> roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> _userManager,
            RoleManager<IdentityRole> _roleManager
            )
        {
            database = context;

            userManager = _userManager;

            roleManager = _roleManager;
        }

        // for displaying all the users
    
        public IActionResult Index()
        {
           IEnumerable<ApplicationUser> allUsers = getAllUsers();
           return View(allUsers);
        }

        // for displaying just one of the users
  
        public IActionResult Show(string id)
        {
            var user = getUserById(id); 
            return View(user);
        }

        // deleting a user 
        [HttpPost]
        public IActionResult Delete(string id)
        {
            var user = getUserById(id);

            // remove user from database and save changes 
            database.Remove(user);
            database.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Profile(string id)
        {
            var user = getUserWithShelvesById(id);

            return View(user);
        }

        // display the form for changing your profile 
        
        public IActionResult Edit(string id)
        {
            var user = getUserById(id);
            return View(user); 
        }

        // update the users profile and redirect to the profile
        [HttpPost]
        public IActionResult Edit(string id, ApplicationUser updatedUserProfile)
        {
            ApplicationUser oldUserProfile = getUserById(id);

            try
            {
               if(ModelState.IsValid)
                {
                    mapUserAttributes(ref oldUserProfile, updatedUserProfile);
                    database.SaveChanges();

                    TempData["message"] = "Your profile was succesfuly updated";

                    return RedirectToAction("Index", "Users");
                }
                else
                {
                    return View(oldUserProfile); 
                }
                
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", "An error occurred while updating your profile: " + ex.Message);
                return View(oldUserProfile);
            }
        }

        private ApplicationUser getUserById(string id)
        {
            return database.Users.Find(id); 
        }

        private ApplicationUser getUserWithShelvesById(string id)
        {
            return database.Users
                    .Include(user => user.Shelves)
                    .FirstOrDefault(user => user.Id == id);
        }

        private IEnumerable<ApplicationUser> getAllUsers()
        {
            List<ApplicationUser> allUsers = database.ApplicationUsers.ToList(); 
            return allUsers;
        }

        private List<Book> getFiveStarBooks(string userId)
        {
            var user = getUserById(userId);

           return database.Reviews
                        .Where(review => review.UserId == userId && review.Rating == 5)
                        .Select(review => review.Book)
                        .ToList();
        }

        private void populateFavouriteBooksOptions(string userId)
        {
            var user = getUserById(userId); 
            List<Book> fiveStarBooks = getFiveStarBooks(userId);

            user.FavoriteBooksOptions = ConvertToSelectList(fiveStarBooks); 
        }

        // converiting a list of books to a select list
        private IEnumerable<SelectListItem> ConvertToSelectList(IEnumerable<Book> books)
        {
            return books.Select(book => new SelectListItem
            {
                Text = book.Title, 
                Value = book.BookId.ToString() 
            });
        }

        // mapping attributes from one user to another 
        private void mapUserAttributes(ref ApplicationUser destination, ApplicationUser source)
        {
            destination.Name = source.Name;
            destination.ProfilePhoto = source.ProfilePhoto; 
        }
    }
}
