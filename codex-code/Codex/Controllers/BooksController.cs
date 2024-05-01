using AngleSharp.Text;
using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Codex.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext database;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public BooksController(ApplicationDbContext context,
            UserManager<ApplicationUser> _userManager,
            RoleManager<IdentityRole> _roleManager)
        {
            database = context;
            userManager = _userManager;
            roleManager = _roleManager;
        }

        private const int booksPerPage = 9;

        // displaying all the books 
        public IActionResult Index(int? page)
        {
            //if theres no current page, we're on page 1
            int pageNumber = page ?? 1;

            // the books that fit on a page
            var paginatedBooks = paginateBooks(pageNumber);

            return View(paginatedBooks);
        }

        // show method for a certain book 
        public IActionResult Show(int id)
        {
            setAccessRights();

            Book book = database.Books
                .Include(b => b.Genre)
                .Include(b => b.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefault(b => b.BookId == id);

            populateShelvesOptions(ref book);

            return View(book);
        }

        // displaying the form for adding a new book
        public IActionResult New()
        {

            Book newBook = new Book();
            newBook.GenreOptions = getAllGenres();

            return View(newBook);
        }

        // adding the info to the database
        [HttpPost]
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
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the book: " + ex.Message);

                // load the genres in order to populate the dropdown list in the view
                newBook.GenreOptions = getAllGenres();

                return View(newBook);
            }
        }

        // displayng the form for editing a book
        public IActionResult Edit(int id)
        {
            // finding the book in the database
            var book = getBookById(id);

            // load the genres in order to populate the dropdown list in the view
            book.GenreOptions = getAllGenres();

            return View(book);

        }

        [HttpPost]
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


        [HttpPost]
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

        [HttpPost]
        public ActionResult AddToShelf(int bookId, string selectedShelfValue)
        {
            // convert string to int 
            int.TryParse(selectedShelfValue, out int shelfId);

            var bookOnShelf = new BookOnShelf
            {
                BookId = bookId,
                ShelfId = shelfId
            };

            database.BooksOnShelves.Add(bookOnShelf);
            database.SaveChanges();

            TempData["message"] = "The book was added to the shelf!";

            return RedirectToAction("Show", "Books", new { id = bookId });


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

        private void setAccessRights()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.IsEditor = User.IsInRole("Editor");
            ViewBag.IsUser = User.IsInRole("User");
            ViewBag.CurrentUser = userManager.GetUserId(User);
        }

        private ApplicationUser getUserWithShelvesById(string id)
        {
            return database.Users
                    .Include(user => user.Shelves)
                    .FirstOrDefault(user => user.Id == id);
        }

        private void populateShelvesOptions(ref Book book)
        {
            var bookId = book.BookId;

            // get the current user id
            var userId = userManager.GetUserId(User);

            // get the shelves of that user
            var userWithShelves = getUserWithShelvesById(userId);

            var shelvesOptions = new List<SelectListItem>();

            foreach (var shelf in userWithShelves.Shelves)
            {
                var bookAlreadOnShelf = database.BooksOnShelves
                    .Where(b => b.BookId == bookId)
                    .Where(s => s.ShelfId == shelf.ShelfId); 

                if(bookAlreadOnShelf != null)
                {
                    var selectListItem = new SelectListItem
                    {
                        Value = shelf.ShelfId.ToString(),
                        Text = shelf.Name,
                        Selected = true
                    };

                    shelvesOptions.Add(selectListItem);
                }
                else
                {
                    var selectListItem = new SelectListItem
                    {
                        Value = shelf.ShelfId.ToString(),
                        Text = shelf.Name,
                        Selected = false
                    };

                    shelvesOptions.Add(selectListItem);
                }

                
            }


            book.ShelvesOptions = shelvesOptions;
        }

    }
}



