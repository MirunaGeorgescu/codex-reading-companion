using Codex.Data;
using Microsoft.AspNetCore.Mvc;

namespace Codex.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext database;

        public ReviewsController(ApplicationDbContext context)
        {
            database = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
