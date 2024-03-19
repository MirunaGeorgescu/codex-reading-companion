using codex_api.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace codex_api.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext db;
        public BooksController(ApplicationDbContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
