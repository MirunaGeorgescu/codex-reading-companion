using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Codex.Controllers
{
    [Route("Books")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext database;

        public BooksController(ApplicationDbContext context)
        {
            database = context;
        }

        private const int booksPerPage = 9;

        // displaying all the books 
        [HttpGet]
        public IActionResult Index(int? page)
        {
            //if theres no current page, we're on page 1
            int pageNumber = page ?? 1; 

            // the books that fit on a page
            var paginatedBooks = paginateBooks(pageNumber);

            return View(paginatedBooks);
        }

        // show method for a certain book 
        [HttpGet("Show/{Id:int}")]
        public IActionResult Show(int id)
        {
            Book book = GetBookByID(id);
            return View(book);
        }

        // displaying the form for adding a new book
        [HttpGet("New")]
        public IActionResult New() { 
            return View(); 
        }

        // adding the info to the database
        [HttpPost("New")]
        public IActionResult New(Book book)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // setting the rating to 0
                    book.Rating = 0;

                    database.Books.Add(book);
                    database.SaveChanges();

                    TempData["message"] = "The book " + book.Title + " was added to the database!";

                    return Redirect("/Books/Show/" + book.BookId);
                }
                else
                {
                    return View(book);
                }
            }
            catch (Exception ex) {
                ModelState.AddModelError("", "An error occurred while saving the book: " + ex.Message);
                return View(book);
            }
           
        }

        // displayng the form for editing a book
        [HttpGet("/Edit/{Id:int}")]
        public IActionResult Edit(int id)
        {
            // finding the book in the database
            var book = GetBookByID(id);

            return View(book); 

        }

        [HttpPost("/Edit/{Id:int}")]
        public IActionResult Edit(int id, Book editedBook)
        {
            Book oldBook = GetBookByID(id);

            if (ModelState.IsValid)
            {
                MapAttributes(ref oldBook, editedBook);
                database.SaveChanges();

                TempData["message"] = "The book " + oldBook.Title + " was succesfully edited!";

                return Redirect("/Books/Show/" + oldBook.BookId);
            }
            else
            {
                return View(oldBook);
            }
        }


        // retrieving all the books from the database
        private IEnumerable<Book> GetAllBooks()
        {
            List<Book> allBooks = database.Books.ToList();
            return allBooks;

        }

        // retrieving a certain book from the database knowing the id
        private Book GetBookByID(int id)
        {
            Book book = database.Books.FirstOrDefault(book => book.BookId == id);
            return book;
        }

        private void MapAttributes(ref Book destination, Book source)
        {
            destination.Title = source.Title;
            destination.Author = source.Author;
            destination.Genre = source.Genre;
            destination.PublicationDate = source.PublicationDate;
            destination.Synopsis = source.Synopsis; 
            destination.CoverImage = source.CoverImage;
        }

        private IEnumerable<Book> paginateBooks(int pageNumber)
        {
            // getting all the books from the database
            var allBooks = GetAllBooks();
            double bookCount = allBooks.Count();

            // calculating the total number of pages
            int totalPages = (int)Math.Ceiling(bookCount / booksPerPage); 

            // calculating how many books to skip based on the number of pages
            int skip = (pageNumber - 1) * booksPerPage;

            // out of the books we skip the ones already displayed on a previous page
            // and return the ones that should be displayed on the current page
            var paginatedBooks = allBooks.Skip(skip).Take(booksPerPage).ToList();

            // seting ViewBag properties for pagination 
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return paginatedBooks;
        }
    }
}



