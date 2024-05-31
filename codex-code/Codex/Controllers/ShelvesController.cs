using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codex.Controllers
{
    public class ShelvesController : Controller
    {

        private readonly ApplicationDbContext database;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public ShelvesController(ApplicationDbContext context,
            UserManager<ApplicationUser> _userManager,
            RoleManager<IdentityRole> _roleManager)
        {
            database = context;
            userManager = _userManager;
            roleManager = _roleManager;
        }

        public IActionResult Show(int shelfId)
        {
            // get name of the shelf in case the shelfToShow has no books 
            var shelf = getShelfById(shelfId);


            var shelfToShow = database.Shelves
                               .Include(s => s.User)
                               .Include(s => s.BooksOnShelves)
                                    .ThenInclude(bos => bos.Book)
                               .FirstOrDefault(s => s.ShelfId == shelfId);


            // setting the current user 
            var currUserId = getCurrentUserId();
            var currentUser = getUserById(currUserId);

            ViewBag.CurrentUser = currentUser;

            return View(shelfToShow);
        }

        private Shelf getShelfById(int shelfId)
        {
            return database.Shelves.FirstOrDefault(s => s.ShelfId == shelfId); 
        }
        private void setAccessRights()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.IsEditor = User.IsInRole("Editor");
            ViewBag.IsUser = User.IsInRole("User");
            ViewBag.CurrentUser = userManager.GetUserId(User);
        }
        private string getCurrentUserId()
        {
            return userManager.GetUserId(User);
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
