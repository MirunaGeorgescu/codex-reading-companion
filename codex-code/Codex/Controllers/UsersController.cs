using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
    
        public IActionResult Index()
        {
           IEnumerable<ApplicationUser> allUsers = getAllUsers();
           return View(allUsers);
        }

        // for displaying just one of the users
  
        public IActionResult Show(string id)
        {
            var user = database.Users
              .Include(u => u.FavoriteBooks)
               .Include(u => u.BadgesEarned)
              .FirstOrDefault(u => u.Id == id);

            populateBadges(ref user);
            return View(user);
        }

        // deleting a user 
        [HttpPost]
        public IActionResult Delete(string id)
        {
            var user = getUserById(id);

            // remove reviews associated with teh user from the database 
            var reviews = GetReviewsByUserId(id);
            
            if(reviews != null)
            {
               // if there are reviews, delete them and update the rating of the book
               foreach(var review in reviews)
               {
                    var book = review.Book;
                    var reviewsLeft = book.Reviews
                            .Where(r => r.UserId != id)
                            .ToList();

                    // if there are reviews left then calculate the new rating
                    if(reviewsLeft.Count> 0)
                    {
                       updateBookRating(-review.Rating, ref book);
                    }
                    else
                    {
                        book.Rating = null; 
                    }
               }
            }
            
            // remove shelves associated with the user from the database 
            var shelves = GetShelvesByUserId(id);

            if (shelves != null)
            {
                database.Shelves.RemoveRange(shelves);
                database.SaveChanges();
            }

            // remove user from database and save changes 
            database.Remove(user);
            database.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Profile(string id)
        {
            // find user, their shelves and their favorite books in the database 
            var user = database.Users
               .Include(u => u.FavoriteBooks)
               .Include(u => u.Shelves)
               .Include(u => u.BadgesEarned)
               .Include(u => u.ReadingChallenges)
               .FirstOrDefault(u => u.Id == id);

            populateBadges(ref user);

            // calculate the progress in the reading challenge 
            var challengeProgress = readingChallengeProgress(user);

            if(challengeProgress != null)
            {
                ViewBag.ChallengeProgress = challengeProgress;
                ViewBag.Year = DateTime.Now.Year;
            }

            return View(user);
        }

        // display the form for changing your profile 
        public IActionResult Edit(string id)
        {
            var user = getUserById(id);

            // add favorite books for the user
            populateFavouriteBooksOptions(ref user);

            return View(user); 
        }

        // update the users profile and redirect to the profile
        [HttpPost]
        public IActionResult Edit(string id, ApplicationUser updatedUserProfile)
        {
            ApplicationUser oldUserProfile = getUserById(id);

            try
            {
               if(ModelState.IsValid)
                {
                    mapUserAttributes(ref oldUserProfile, updatedUserProfile);

                    updateFavoriteBooksSelection(oldUserProfile.Id, Request.Form["selectedFavoriteBooks"]); 
                    database.SaveChanges();

                    TempData["message"] = "Your profile was succesfuly updated";

                    return RedirectToAction("Index", "Users");
                }
                else
                {
                    // add favorite books for the user
                    populateFavouriteBooksOptions(ref oldUserProfile);

                    return View(oldUserProfile); 
                }
                
            }
            catch (Exception ex)
            {
                // add favorite books for the user
                populateFavouriteBooksOptions(ref oldUserProfile);

                ModelState.AddModelError("", "An error occurred while updating your profile: " + ex.Message);
                return View(oldUserProfile);
            }
        }

        private ApplicationUser getUserById(string id)
        {
            return database.Users.Find(id); 
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

        private IEnumerable<Review> GetReviewsByUserId(string userId)
        {
            return database.Reviews.Where(review => review.UserId == userId);
        }

        private IEnumerable<Shelf> GetShelvesByUserId(string userId)
        {
            return database.Shelves.Where(shelf => shelf.UserId == userId);
        }

        private void populateFavouriteBooksOptions(ref ApplicationUser user)
        {
            List<Book> fiveStarBooks = getFiveStarBooks(user.Id);

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

        // mapping attributes from one user to another 
        private void mapUserAttributes(ref ApplicationUser destination, ApplicationUser source)
        {
            destination.Name = source.Name;
            destination.ProfilePhoto = source.ProfilePhoto; 
        }

        private void updateBookRating(int rating, ref Book book)
        {
            // finding if there are any other reviews in order to calculate the books rating
            int reviewsCount = getReviewsByBookId(book.BookId).Count();

            // if there are ratings already, update the average rating  
            if (reviewsCount > 0)
            {
                book.Rating = ((int)(((book.Rating * reviewsCount) + rating) / (reviewsCount + 1) * 10)) / (float)10;
            }
            else
            {
                book.Rating = rating;
            }

        }

        private List<Review> getReviewsByBookId(int bookId)
        {
            return database.Reviews.Where(review => review.BookId == bookId).ToList();
        }

        private void updateFavoriteBooksSelection(string userId, string[] selectedBooks)
        {
            var user = database.Users
                .Include(u => u.FavoriteBooks)
                .FirstOrDefault(u => u.Id == userId);

            // if the user has not set any favorite books yet, initialize the list 
            if (user.FavoriteBooks != null)
            {
                user.FavoriteBooks.Clear();
            }
            else
            {
                user.FavoriteBooks = new List<Book>(); 
            }
           

            if (selectedBooks != null)
            {
                // find the selected favorite books and add them to the list
                foreach (var selectedBook in selectedBooks)
                {
                    var bookId = int.Parse(selectedBook);
                    var book = database.Books
                        .FirstOrDefault(b => b.BookId == bookId); 

                    if(book != null)
                    {
                        user.FavoriteBooks.Add(book); 
                    }
                }

                database.SaveChanges();             
            }
        }

        private void populateBadges( ref ApplicationUser user)
        {
            var badges = new List<ReadingBadge>();

            foreach(var badgeEarned in user.BadgesEarned)
            {
                int badgeId =(int) badgeEarned.BadgeId; 

                var badge = database.ReadingBadges
                                .FirstOrDefault(rb => rb.BadgeId == badgeId);

                badges.Add(badge);
            }

            user.Badges = badges;
        }
      
        private double? readingChallengeProgress(ApplicationUser user)
        {
          if(user.HasJoinedChallenge(new ReadingChallenge { StartDate = new DateOnly(DateTime.Now.Year, 1, 1), UserId = user.Id }))
          {
                // find the reading challenge 
                var readingChallenge = database.ReadingChallenges
                                            .FirstOrDefault(rc => rc.UserId == user.Id
                                                                && rc.StartDate.Year == DateTime.Now.Year);

                // find how many books the user wants to read 
                var targetBooks = readingChallenge.TargetNumber;

                // find how many books the user has read
                var booksRead = 0; 
                if (readingChallenge.BooksRead != null)
                {
                    booksRead = readingChallenge.BooksRead.Count;
                }

                // calculate progress procentage
                return (double)booksRead / targetBooks * 100;
            }

            // if the user hasn't joined the reading challenge then return null
            return null; 
        }
    }
}
