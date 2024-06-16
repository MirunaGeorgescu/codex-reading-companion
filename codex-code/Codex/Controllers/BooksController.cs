using AngleSharp.Text;
using Codex.Data;
using Codex.Models;
using Codex.Enums; 
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Diagnostics.Metrics;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;

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
            setAccessRights();

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


            var userId = userManager.GetUserId(User);
            if(userId != null)
            {
                populateShelvesOptions(ref book);
            }

            return View(book);
        }


        // displaying the form for adding a new book
        [Authorize(Roles = "Admin")]
        public IActionResult New()
        {

            Book newBook = new Book();
            newBook.GenreOptions = getAllGenres();

            return View(newBook);
        }

        // adding the info to the database
        [Authorize(Roles = "Admin")]
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
                        return RedirectToAction("Show", new {id = newBook.BookId});
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
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id)
        {
            // finding the book in the database
            var book = getBookById(id);

            // load the genres in order to populate the dropdown list in the view
            book.GenreOptions = getAllGenres();

            return View(book);

        }

        [Authorize(Roles = "Editor,Admin")]
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

        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            // find book based on id
            Book book = getBookById(id);

            // find the book on the shelves of people and detelte it from there
            var bookOnShelves = database.BooksOnShelves
                .Where(bos => bos.BookId == id);
            database.BooksOnShelves.RemoveRange(bookOnShelves);

            // find the reviews asociated with that book and delete them form teh db 
            var bookReviews = database.Reviews.Where(review => review.BookId == id);
            database.Reviews.RemoveRange(bookReviews);

            // remove book form database
            database.Books.Remove(book);

            TempData["message"] = "The book " + book.Title + " was deleted from the database!";

            // saving the changes made 
            database.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public ActionResult AddToShelf(int bookId, string selectedShelfValue)
        {
            // convert string to int 
            int.TryParse(selectedShelfValue, out int shelfId);

            // if the owner of the shelf has already added that book on one
            // of the shelves, then delete the entry in the table 
            var alreadyOnShelfId = bookAlreadyOnShelf(bookId, shelfId);
            Shelf? oldShelf = null; 


            if (alreadyOnShelfId != -1)
            {
                var book = database.BooksOnShelves
                    .FirstOrDefault(bos => bos.ShelfId == alreadyOnShelfId && bos.BookId == bookId);

                // store the shelf in order to check between which shelves the books are moving
                oldShelf = database.Shelves
                             .Include(s => s.User)
                             .FirstOrDefault(s => s.ShelfId == alreadyOnShelfId);

                database.BooksOnShelves.Remove(book);
            }

            var bookOnShelf = new BookOnShelf
            {
                BookId = bookId,
                ShelfId = shelfId
            };


            var newShelf = database.Shelves
                     .Include(s => s.User)
                     .FirstOrDefault(s => s.ShelfId == bookOnShelf.ShelfId);

            var user = newShelf.User;
            var userId = user.Id;
            user = database.Users
                 .Include(u => u.ReadingChallenges)
                 .FirstOrDefault(u => u.Id == userId);

            // check if the user is part of the reading challenge 
            if (user.HasJoinedChallenge(new ReadingChallenge { StartDate = new DateOnly(DateTime.Now.Year, 1, 1), UserId = user.Id }))
           {
                // book is getting moved from a shelf to another
                if (oldShelf != null)
                {
                    // if its getting moved from currently reading to read
                    if (oldShelf.Name == "Currently reading" && newShelf.Name == "Read")
                    {
                        var book = getBookById(bookId);

                        userReadBook(user, book);
                    }

                    // if the book gets removed from read it gets deleted from that years reading challange if it was there
                    if(oldShelf.Name == "Read")
                    {
                        var book = getBookById(bookId);
                        removeBookFromReadingChallenge(user, book);
                    }
                }
           }

            database.BooksOnShelves.Add(bookOnShelf);
            database.SaveChanges();

            TempData["message"] = "The book was added to the shelf!";


            // check if the user can get a new badge
            user = getUserByShelfId(shelfId); 

            if (user != null)
            {
                var badges = database.ReadingBadges
                                .ToList();

                foreach (var badge in badges)
                {
                    if (meetsBadgeCriteria(user, badge))
                        awardBadge(user.Id, badge.BadgeId);
                }
            }

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
            Book book = database.Books
                .FirstOrDefault(book => book.BookId == id);
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
            return getBookByTitle(book.Title) == null;
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

        // checks if the given book id is on one of the shelves owend by the user that owns the shelf with shelfId
        // if it can be found it returnes the id of that shelf, if not -1
        private int bookAlreadyOnShelf(int bookId, int shelfId)
        {
            var shelf = database.Shelves.FirstOrDefault(s => s.ShelfId == shelfId);
            var userId = shelf.UserId;
            var userShelves = getShelvesByUserId(userId);

            foreach (var userShelf in userShelves)
            {
                var alreadyExistingBook = isSelected(bookId, userShelf.ShelfId);

                if (alreadyExistingBook)
                {
                    return userShelf.ShelfId;
                }
            }

            return -1;
        }

        private List<Shelf> getShelvesByUserId(string userId)
        {
            return database.Shelves
                .Where(s => s.UserId == userId)
                .ToList();
        }

        private bool isSelected(int bookId, int shelfId)
        {
            return database.BooksOnShelves.Any(bos => bos.BookId == bookId && bos.ShelfId == shelfId);
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

                var selectListItem = new SelectListItem
                {
                    Value = shelf.ShelfId.ToString(),
                    Text = shelf.Name,
                    Selected = isSelected(bookId, shelf.ShelfId)
                };

                shelvesOptions.Add(selectListItem);
            }

            book.ShelvesOptions = shelvesOptions;
        }

        private bool meetsBadgeCriteria(ApplicationUser user, ReadingBadge badge)
        {
            // get the criteria information 
            var criteriaType = badge.CriteriaType;


            switch(criteriaType)
            {
                case CriteriaType.BooksRead:
                    return meetsBooksReadCriteria(user, badge); 

                case CriteriaType.BooksToRead:
                    return meetsBooksToReadCriteria(user, badge); 

                case CriteriaType.GenreCount:
                    return meetsGenreCountCriteria(user, badge);

                case CriteriaType.AuthorCount:
                    return meetsAuthorCountCriteria(user, badge);

                default:
                    return false;
            }
        }

        private bool meetsBooksReadCriteria(ApplicationUser user, ReadingBadge badge)
        {
            // check if the user has read the number of books 

            int numberOfBooks = badge.CriteriaValue;

            // get the users read shelf
            var readShelf = getShelfByNameForUser(user.Id, "Read");

            // count the books on the read shelf
            var booksRead = readShelf.BooksOnShelves
                                .Where(bos => bos.ShelfId == readShelf.ShelfId)
                                .Count();

            // check if the criteria is met 
            return booksRead >= numberOfBooks;
        }

        private bool meetsBooksToReadCriteria(ApplicationUser user, ReadingBadge badge)
        {
            // check if the user has a certain number of books on their tbr 

            int numberOfBooks = badge.CriteriaValue;

            // get the users read shelf
            var TBRShelf = getShelfByNameForUser(user.Id, "Want to read");

            // count the books on the tbr shelf 
            var TBRBooks = TBRShelf.BooksOnShelves
                            .Where(bos => bos.ShelfId == TBRShelf.ShelfId)
                            .Count();

            return TBRBooks >= numberOfBooks;

        }

        private bool meetsGenreCountCriteria(ApplicationUser user, ReadingBadge badge)
        {
            // user has read a certain amount of books in that genre

            int numberOfBooks = badge.CriteriaValue;
            string genreName = badge.TargetName;

            int genreCount = 0;

            // get the read shelf of the user
            var readShelf = getShelfByNameForUser(user.Id, "Read");

            if (readShelf != null)
            {
                // go through the books on the read shelf and check the genre
                foreach (var bookOnShelf in readShelf.BooksOnShelves)
                {
                    int bookId = (int)bookOnShelf.BookId;

                    // get the book from the database in order to check the genre
                    var book = getBookById(bookId);

                    if (book != null)
                    {
                        // check to see if the genre of the book is classic
                        var genreID = book.GenreId;
                        var genre = database.Genres.FirstOrDefault(g => g.GenreId == genreID);
                        if (genre != null && genre.Name == genreName)
                            genreCount++;
                    }
                }

            }

            return genreCount >= numberOfBooks;

        }

        private bool meetsAuthorCountCriteria(ApplicationUser user, ReadingBadge badge)
        {
            // user has read a certain amount of books by the author 
            int numberOfBooks = badge.CriteriaValue;
            string authorName = badge.TargetName;

            int authorCount = 0;

            // get the read shelf of the user
            var readShelf = getShelfByNameForUser(user.Id, "Read");

            if (readShelf != null)
            {
                // go through the books on the read shelf and check the author
                foreach (var bookOnShelf in readShelf.BooksOnShelves)
                {
                    var bookId = (int)bookOnShelf.BookId;
                    var book = getBookById(bookId); 

                    if(book != null && book.Author == authorName)
                        authorCount++;
                }
            }

            return authorCount >= numberOfBooks;
        }

        private void awardBadge(string userId, int badgeId)
        {
            // find the user in the database
            var user = getUserById(userId);

            // find badge the database
            var badge = getBadgeById(badgeId);

            // check to see if the user has already earned that badge before 
            if (!hasBadge(user, badge))
            {
                var badgeEarned = new BadgeEarned
                {
                    UserId = userId,
                    BadgeId = badgeId,
                    DateEarned = DateTime.Now
                };

                database.BadgesEarned.Add(badgeEarned);
                database.SaveChanges();
            }

            TempData["badgeMessage"] = "Congratulations! You earned a new badge!";
        }

        private ApplicationUser getUserById(string id)
        {
            return database.Users.Find(id);
        }

        private bool hasBadge(ApplicationUser user, ReadingBadge badge)
        {
            var readingBadge = database.BadgesEarned
                                .FirstOrDefault(be => be.UserId == user.Id && be.BadgeId == badge.BadgeId);

            return readingBadge != null;
        }

        private ReadingBadge getBadgeById(int id)
        {
            return database.ReadingBadges
                .FirstOrDefault(rb => rb.BadgeId == id);
        }

        private Shelf getShelfByNameForUser(string userId, string shelfName)
        {
            var user = database.Users
                        .Include(u => u.Shelves)
                        .ThenInclude(s => s.BooksOnShelves)
                        .FirstOrDefault(u => u.Id == userId);

            return user.Shelves.FirstOrDefault(s => s.Name == shelfName);
        }

        private ApplicationUser getUserByShelfId (int shelfId)
        {
          var shelf = database.Shelves
                .Where(s => s.ShelfId == shelfId)
                .Include(s => s.User)
                .FirstOrDefault();

            return shelf.User; 
        }

        private void userReadBook(ApplicationUser user, Book book)
        {
           var userId = user.Id;
           user = database.Users
                .Include(u => u.ReadingChallenges)
                .FirstOrDefault(u => u.Id == userId);

            var readingChallenge = user.ReadingChallenges
                                        .FirstOrDefault(rc => rc.StartDate.Year == DateTime.Now.Year 
                                           && rc.UserId == userId);

            if(readingChallenge.BooksRead == null)
            {
                readingChallenge.BooksRead = new List<Book>();
            }

            readingChallenge.BooksRead.Add(book);

            database.SaveChanges();
        }

        private void removeBookFromReadingChallenge(ApplicationUser user, Book book)
        {
            var userId = user.Id;
            user = database.Users
                 .Include(u => u.ReadingChallenges)
                 .FirstOrDefault(u => u.Id == userId);

            var readingChallenge = user.ReadingChallenges
                                        .FirstOrDefault(rc => rc.StartDate.Year == DateTime.Now.Year
                                           && rc.UserId == userId);

            if (readingChallenge.BooksRead != null)
            {
                var bookToRemove = readingChallenge.BooksRead
                                       .FirstOrDefault(b => b.BookId == book.BookId);

                if (bookToRemove != null)
                {
                    readingChallenge.BooksRead.Remove(bookToRemove);
                    database.SaveChanges();
                }
            }
        }
    }
}



