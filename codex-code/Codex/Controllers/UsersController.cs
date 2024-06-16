using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Protocol.Plugins;
using System.Net;
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
        [Authorize(Roles = "Editor,Admin,User")]
        public IActionResult Index()
        {
           IEnumerable<ApplicationUser> allUsers = getAllUsers();
           return View(allUsers);
        }

        // for displaying just one of the users
        [Authorize(Roles = "Editor,Admin,User")]
        public IActionResult Show(string id)
        {
            var user = database.Users
              .Include(u => u.FavoriteBooks)
               .Include(u => u.BadgesEarned)
               .Include(u => u.Followers)
               .Include(u => u.Following)
              .FirstOrDefault(u => u.Id == id);

            populateBadges(ref user);

            // get the current user in order to know which button to display
            var currUserId = getCurrentUserId();
            var currentUser = getUserById (currUserId);

            ViewBag.IsFollowing = isFollowing(currentUser, user);

            ViewBag.today = DateTime.Now;
            ViewBag.yesterday = DateTime.Now.AddDays(-1);

            setAccessRights(); 

            return View(user);
        }

        // deleting a user 
        [Authorize(Roles = "Editor,Admin,User")]
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

        [Authorize(Roles = "Editor,Admin,User")]
        public IActionResult Profile(string id)
        {
            // find user, their shelves and their favorite books in the database 
            var user = database.Users
               .Include(u => u.FavoriteBooks)
               .Include(u => u.Shelves)
               .Include(u => u.BadgesEarned)
               .Include(u => u.ReadingChallenges)
               .Include(u => u.Followers)
               .Include(u => u.Following)
               .Include(u => u.FriendsQuestsAsUser1)
               .Include(u => u.FriendsQuestsAsUser2)
               .FirstOrDefault(u => u.Id == id);

            populateBadges(ref user);

            // READING CHALLANGE
            // calculate the progress in the reading challenge 
            var challengeProgress = readingChallengeProgress(user);

            if(challengeProgress != null)
            {
                ViewBag.ChallengeProgress = challengeProgress;
                ViewBag.Year = DateTime.Now.Year;
            }

            ViewBag.today = DateTime.Now;
            ViewBag.yesterday = DateTime.Now.AddDays(-1);

            // FRIENDS QUEST
            // find if user is in a friends quest 
            var currentFriendsQuest = database.FriendsQuests
                                         .FirstOrDefault(fq => (fq.UserId1 == id || fq.UserId2 == id)
                                                                    && fq.EndDate > DateTime.Now);
            if(currentFriendsQuest !=  null)
            {
                ViewBag.IsInFriendsQuest = true;
                ViewBag.TargetPages = currentFriendsQuest.TargetPages; 

                var progress = (double)currentFriendsQuest.PagesRead / currentFriendsQuest.TargetPages * 100;
                ViewBag.FriendsQuestProgress = Math.Min(progress, 100);

                // find the other user
                var user1 = currentFriendsQuest.User1;
                var user2 = currentFriendsQuest.User2;

                // if the current user is user1 then the other friend in the quest is user 2
                if(user1.Id == id)
                {
                    ViewBag.FriendInQuest = user2.Name;
                }
                else
                {
                    ViewBag.FriendInQuest = user1.Name;
                }
                


            }
            else
                ViewBag.IsInFriendsQuest = false;

            setAccessRights();

            if (id == userManager.GetUserId(User) || User.IsInRole("Admin") || User.IsInRole("Editor"))
                ViewBag.IsAllowed = true; 
            else
                ViewBag.IsAllowed = false;

            

            return View(user);
        }

        // display the form for changing your profile 
        [Authorize(Roles = "Editor,Admin,User")]
        public IActionResult Edit(string id)
        {
            var user = getUserById(id);

            // add favorite books for the user
            populateFavouriteBooksOptions(ref user);

            return View(user); 
        }

        // update the users profile and redirect to the profile
        [Authorize(Roles = "Editor,Admin,User")]
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

        [Authorize(Roles = "User")]
        public IActionResult Follow(string followedUserId)
        {
            var followerUserId = getCurrentUserId();

            var followerUser = getUserById(followerUserId);
            var followedUser = getUserById(followedUserId);

            if(followedUser != null && followerUser != null)
            {
                // if they are not following them already, add them 
                if(!followerUser.Following.Contains(followedUser))
                {
                    followerUser.Following.Add(followedUser); 
                }

                if (!followedUser.Followers.Contains(followerUser))
                {
                    followedUser.Followers.Add(followerUser);
                }

                database.SaveChanges();
            }

            return RedirectToAction("Show", "Users", new { id = followedUserId });
        }

        [Authorize(Roles = "User")]
        public IActionResult Unfollow(string unfollowedUserId)
        {
            var followerUserId = getCurrentUserId();

            var followerUser = getUserById(followerUserId);
            var unfollowedUser = getUserById(unfollowedUserId);

            if(unfollowedUser != null && followerUser != null)
            {
                // take them out of the lists 
                if (followerUser.Following.Contains(unfollowedUser))
                {
                    followerUser.Following.Remove(unfollowedUser);
                }


                if (unfollowedUser.Followers.Contains(followerUser))
                {
                    unfollowedUser.Followers.Remove(followerUser);
                }

                database.SaveChanges();
            }

            return RedirectToAction("Show", "Users", new { id = unfollowedUserId });
        }

        [Authorize(Roles = "User")]
        public IActionResult UpdateProgress(string userId, int bookId)
        {
            ApplicationUser user = getUserById(userId);
            Book book = getBookById(bookId);

            // find the currently reading shelf 
            Shelf currentlyReadingShelf = database.Shelves
                                              .FirstOrDefault(s => s.Name == "Currently reading" 
                                                                    && s.UserId == userId);  
            // find the book on shelf
            BookOnShelf bookOnShelf = database.BooksOnShelves
                                            .Include(bos => bos.Book)
                                            .Include(bos => bos.Shelf)
                                            .FirstOrDefault(bos => bos.BookId ==  bookId 
                                                && bos.ShelfId == currentlyReadingShelf.ShelfId);


            return View(bookOnShelf);
        }


        [Authorize(Roles = "User")]
        [HttpPost]
        public IActionResult UpdateProgress(string userId, int bookId, BookOnShelf update)
        {
            // find the currently reading shelf 
            Shelf currentlyReadingShelf = database.Shelves
                                              .FirstOrDefault(s => s.Name == "Currently reading"
                                                                    && s.UserId == userId);

            // find the book on shelf 
            BookOnShelf oldBookOnShelf = database.BooksOnShelves
                                            .Include(bos => bos.Book)
                                            .Include(bos => bos.Shelf)
                                            .FirstOrDefault(bos => bos.BookId == bookId
                                                && bos.ShelfId == currentlyReadingShelf.ShelfId);

            if(update.CurrentPage < 0)
            {
                ModelState.AddModelError("CurrentPage", "The updated page can't be negative!");
                return View(oldBookOnShelf);
            }

            if (update.CurrentPage <= oldBookOnShelf.CurrentPage)
            {
                ModelState.AddModelError("CurrentPage", "The updated page should be greater than the current page!");
                return View(oldBookOnShelf);
            }

            // find book 
            var book = getBookById(bookId);

            if(book.NumberOfPages < update.CurrentPage) {
                ModelState.AddModelError("CurrentPage", "The updated page should be less than the total number of pages!");
                return View(oldBookOnShelf);
            }

            // if the user has finished the book, move book to the read shelf 
            if (book.NumberOfPages == update.CurrentPage)
            {
                ModelState.AddModelError("CurrentPage", "If you finished the book, change the shelf from the book details!");
                return View(oldBookOnShelf);
            }



            var pagesRead = update.CurrentPage - oldBookOnShelf.CurrentPage;

            // update the current page 
            oldBookOnShelf.CurrentPage = update.CurrentPage;

            database.SaveChanges();

            // find user
            var user = getUserById(userId);

            DateTime today = DateTime.Now;

            // streak tracking 
            // check if user has already read anything today
            if (sameDate(today, user.LastUpdate) && user.PagesReadToday != 0)
            {
                user.PagesReadToday += (int)pagesRead;
            }
            else
            {
                user.PagesReadToday = (int)pagesRead;
                user.LastUpdate = today;
            }

           

            // check if the user has read more than 30 pages today and set the streak
            if (user.PagesReadToday >= 30 )
            {
                DateTime yesterday = today.AddDays(-1);

                if (sameDate(yesterday, user.LastStreakDay))
                {
                    user.LastStreakDay = today;
                    user.Streak++;
                }
                else
                {
                    user.LastStreakDay = today;
                    user.Streak = 1;
                }
            }

            // add the progress to the friends quest too, if the user is part of one 
            var currentFriendsQuest = database.FriendsQuests
                                        .FirstOrDefault(fq => (fq.UserId1 == userId || fq.UserId2 == userId)
                                                                   && fq.EndDate > DateTime.Now);

            if(currentFriendsQuest != null)
            {
                currentFriendsQuest.PagesRead += (int)pagesRead; 
                database.SaveChanges();
            }

            return RedirectToAction("Show", "Shelves", new { shelfId = oldBookOnShelf.ShelfId });
        }


        private ApplicationUser getUserById(string id)
        {
            return database.Users
                 .Include(u => u.Followers)
                 .Include(u => u.Following)
                .FirstOrDefault(u => u.Id == id); 
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
                                            .Include(rc => rc.BooksRead)
                                            .FirstOrDefault(rc => rc.UserId == user.Id
                                                                && rc.StartDate.Year == DateTime.Now.Year);

                ViewBag.ChallangeId = readingChallenge.ReadingChallengeId; 

                 // find how many books the user wants to read 
                 var targetBooks = readingChallenge.TargetNumber;

                // find how many books the user has read
                var booksRead = 0; 
                if (readingChallenge.BooksRead != null)
                {
                    booksRead = readingChallenge.BooksRead.Count;
                }

                // calculate progress procentage
                return Math.Round((double)booksRead / targetBooks * 100, 2);
            }

            // if the user hasn't joined the reading challenge then return null
            return null; 
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

        private bool isFollowing(ApplicationUser user1, ApplicationUser user2)
        {
            var followingList = user1.Following.ToList(); 

            foreach(var user in followingList)
            {
                if(user2.Id == user.Id)
                    return true;
            }

            return false;
        }

        private bool isFollowedBy(ApplicationUser user1, ApplicationUser user2)
        {
            var followingList = user1.Followers.ToList();

            foreach (var user in followingList)
            {
                if (user2.Id == user.Id)
                    return true;
            }

            return false;
        }

        private Book getBookById(int id)
        {
            Book book = database.Books
                .FirstOrDefault(book => book.BookId == id);
            return book;
        }

        private bool sameDate(DateTime date1, DateTime date2)
        {
            if(date1 == date2) return true;

            if(date1.Year == date2.Year)
                if(date1.Month == date2.Month)
                    if(date1.Day == date2.Day)
                        return true;

            return false; 
        }


    }
}
