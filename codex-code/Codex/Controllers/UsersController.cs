using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        [HttpGet]
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
            var user = getUserById(id);

            return View(user);
        }

        private ApplicationUser getUserById(string id)
        {
            return database.ApplicationUsers.FirstOrDefault(user => user.Id == id); 
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
    }
}
