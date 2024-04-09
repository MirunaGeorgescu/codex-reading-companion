using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Codex.Controllers
{
    [Route("books")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext database;

        public BooksController(ApplicationDbContext context)
        {
            database = context;
        }

        public IActionResult Index()
        {



            return View();
        }

        [HttpGet("Show/{Id:int}")]
        public IActionResult Show(int id)
        {
            Book book = GetBookByID(id);
            return View(book);
        }

        public IEnumerable<Book> GetAllBooks()
        {
            List<Book> allBooks = database.Books.ToList();
            return allBooks;

        }

        public Book GetBookByID(int id)
        {
            Book book = database.Books.FirstOrDefault(book => book.BookId == id);
            return book;
        }
    }
}



