using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        [HttpGet("Show/{id:int}")]
        public IActionResult Show(int id)
        {
            Book book = database.Books
                .Include(b => b.Genre)
                .Include(b => b.Reviews)
                .FirstOrDefault(b => b.BookId == id);

            return View(book);
        }

        // displaying the form for adding a new book
        [HttpGet("New")]
        public IActionResult New() {
           
            Book newBook = new Book();
            newBook.GenreOptions = getAllGenres(); 

            return View(newBook);
        }

        // adding the info to the database
        [HttpPost("New")]
        public IActionResult New(Book newBook)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (isBookUnique(newBook))
                    {
                        // if the book is not already in the database we add it 
                        database.Books.Add(newBook);
                        database.SaveChanges();

                        // show the book 
                        return Redirect("/Books/Show/" + newBook.BookId);
                    }
                    else
                    {
                        // if the book is already in the system, then we throw an error 
                        ModelState.AddModelError("Book", "The book " + newBook.Title + " already exists in the database!");

                        // load the genres in order to populate the dropdown list in the view
                        newBook.GenreOptions = getAllGenres();
                        return View(newBook);
                    }
                }
                else
                {
                    // load the genres in order to populate the dropdown list in the view
                    newBook.GenreOptions = getAllGenres();
                    return View(newBook);
                }
            }
            catch (Exception ex) {
                ModelState.AddModelError("", "An error occurred while saving the book: " + ex.Message);

                // load the genres in order to populate the dropdown list in the view
                newBook.GenreOptions = getAllGenres();

                return View(newBook);
            }
        }

        // displayng the form for editing a book
        [HttpGet("Edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            // finding the book in the database
            var book = getBookById(id);

            // load the genres in order to populate the dropdown list in the view
            book.GenreOptions = getAllGenres();

            return View(book); 

        }

        [HttpPost("Edit/{id:int}")]
        public IActionResult Edit(int id, Book editedBook)
        {
            Book oldBook = getBookById(id);

            try
            {
                if (isBookUnique(editedBook))
                {
                    if (ModelState.IsValid)
                    {
                        mapBookAttributes(ref oldBook, editedBook);
                        database.SaveChanges();

                        TempData["message"] = "The book " + oldBook.Title + " was succesfully edited!";

                        return Redirect("/Books/Show/" + oldBook.BookId);
                    }
                    else
                    {
                        return View(oldBook);
                    }
                }
                else
                {
                    // if the book is not unique then throw error
                    ModelState.AddModelError("Book", "The book " + editedBook.Title + " already exists in the database!");
                    return View(oldBook);
                }
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", "An error occurred while saving the book: " + ex.Message);
                return View(oldBook);
            }
        }


        [HttpPost("Delete/{id:int}")]
        public ActionResult Delete(int id)
        {
            // find book based on id
            Book book = getBookById(id);

            // remove book form database
            database.Books.Remove(book);

            TempData["message"] = "The book " + book.Title + " was deleted from the database!"; 

            // saving the changes made 
            database.SaveChanges();
            return RedirectToAction("Index");
        }

        // retrieving all the books from the database
        private IEnumerable<Book> getAllBooks()
        {
            List<Book> allBooks = database.Books.ToList();
            return allBooks;

        }

        // retrieving a certain book from the database knowing the id
        private Book getBookById(int id)
        {
            Book book = database.Books.FirstOrDefault(book => book.BookId == id);
            return book;
        }

        private Book getBookByTitle(string title)
        {
            Book book = database.Books.FirstOrDefault(book => book.Title == title);
            return book; 
        }

        private void mapBookAttributes(ref Book destination, Book source)
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
            var allBooks = getAllBooks();
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

        [NonAction]
        private IEnumerable<SelectListItem> getAllGenres()
        {
            // making a new list of selectItems
            var selectList = new List<SelectListItem>();

            var allGenresFormDatabase = database.Genres.ToList();

            // transforming all the genres in selectListItems
            foreach (var genre in allGenresFormDatabase)
            {
                var selectItem = new SelectListItem();
                selectItem.Value = genre.GenreId.ToString();
                selectItem.Text = genre.Name;

                selectList.Add(selectItem);
            }

            return selectList;
        }

        private bool isBookUnique(Book book)
        {
            return getBookByTitle(book.Title) != null;
        }

    }
}



