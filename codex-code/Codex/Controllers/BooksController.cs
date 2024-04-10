﻿using Codex.Data;
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

        // retrieving all the books from the database
        public IEnumerable<Book> GetAllBooks()
        {
            List<Book> allBooks = database.Books.ToList();
            return allBooks;

        }

        // retrieving a certain book from the database knowing the id
        public Book GetBookByID(int id)
        {
            Book book = database.Books.FirstOrDefault(book => book.BookId == id);
            return book;
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


