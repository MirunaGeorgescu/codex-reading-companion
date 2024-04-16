using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
           return View( allUsers );

        }

        private IEnumerable<ApplicationUser> getAllUsers()
        {
            List<ApplicationUser> allUsers = database.ApplicationUsers.ToList(); 
            return allUsers;
        }
    }
}
