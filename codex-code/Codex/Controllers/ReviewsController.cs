using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Codex.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext database;
        //private readonly UserManager<IdentityUser> userManager;
        //private readonly RoleManager<IdentityRole> roleManager;

        public ReviewsController(ApplicationDbContext context)
        {
            database = context;
        }

        // displaying the form for a new comment 
        [HttpGet]
       public IActionResult New(int bookId)
        {
            // making sure the user has not already left a review for the book
            string userId = null; 
            if(!hasAlreadyReviewedBook(userId, bookId))
            {
                Review newReview = new Review();
                newReview.BookId = bookId;
                newReview.Book = getBookById(bookId);
                return View(newReview);
            }
            else
            {
                TempData["message"] = "You have already left a review for this book!";
                return RedirectToAction("Show", "Books", new { id = bookId });
            }
        }

        [HttpPost]
        public IActionResult New(int bookId, Review newReview)
        {
            if(ModelState.IsValid)
            {
                // finding the book that the review was written for
                var book = getBookById(bookId);
                
                // update the rating for the book 
                updateBookRating(newReview.Rating, ref book);

                // setting the review date
                newReview.Date = DateTime.Now;

                // setting the user who left the review 
                //newReview.UserId = userManager.GetUserId(User);

                // adding the new review to the database and saving the changes 
                database.Add(newReview);
                database.SaveChanges();

                TempData["message"] = "Your review was succesfully added!"; 

                return Redirect("/books/show/" + bookId);
            }
            else
            {
                return View(newReview);
            }
        }

        // displaying the edit form for a book
        [HttpGet("Reviews/Edit/{reviewId:int}")]
        public IActionResult Edit(int reviewId)
        
        
        {
            Review review  = GetReviewById(reviewId);
            return View(review);
        }

        [HttpPost]
        public IActionResult Edit(int reviewId, Review updatedReview)
        {
            Review existingReview = GetReviewById(reviewId);

            try
            {
                if (ModelState.IsValid)
                {
                    // finding the book in the database in order to be able to update the rating
                    int bookId = (int)existingReview.BookId;
                    Book book = getBookById(bookId);

                    // updating the review
                    mapReviewAttributes(ref existingReview, updatedReview);

                    // updating the rating of the book if the rating has changed 
                    if(existingReview.Rating != updatedReview.Rating)
                    {
                        updateBookRating(updatedReview.Rating, ref book);
                    }

                    database.SaveChanges();
                    TempData["message"] = "Your review was successfully updated!";

                    return RedirectToAction("Show", "Books", new { id = bookId });
                }
                else
                {
                    return View(updatedReview);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the review: " + ex.Message);
                return View(updatedReview); 
            }


        }

        [HttpPost("Reviews/Delete/{reviewId:int}")]
        public ActionResult Delete(int reviewId)
        {
            // find review in the database 
            Review review = GetReviewById(reviewId); 

            // remove the review form the database
            database.Reviews.Remove(review);


            // update the book rating after deleting the review 
            Book book = getBookById((int)review.BookId);
            updateBookRating(-review.Rating, ref book); 

            database.SaveChanges();

            TempData["message"] = "Your review was successfully deleted!";

            return RedirectToAction("Show", "Books", new { id = review.BookId });
        }


        private Book getBookById(int bookId)
        {
            Book book = database.Books
                .Include(book => book.Genre)
                .FirstOrDefault(book => book.BookId == bookId);
            return book;
        }

        private List<Review> getReviewsByBookId(int bookId)
        {
            return database.Reviews.Where(review => review.BookId ==  bookId).ToList();
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

        private bool hasAlreadyReviewedBook(string userId, int bookId)
        {
            if (userId == null)
            {
                return false;
            }
            else
            {
                Review alreadyExistingReview = database.Reviews.FirstOrDefault(review => review.UserId == userId && review.BookId == bookId);

                return alreadyExistingReview != null;
            }
        }

        private Review GetReviewById(int reviewId)
        {
            return database.Reviews.FirstOrDefault(review => review.ReviewId == reviewId); 
        }

        private void mapReviewAttributes(ref Review destination, Review source)
        {
            destination.Rating = source.Rating;
            destination.Comment = source.Comment;
        }
    }
}
