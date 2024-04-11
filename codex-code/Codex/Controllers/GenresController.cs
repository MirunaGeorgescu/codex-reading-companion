using Codex.Data;
using Codex.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Codex.Controllers
{
    public class GenresController : Controller
    {
        private readonly ApplicationDbContext database;

        public GenresController(ApplicationDbContext context)
        {
            database = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // getting all the genres from the database 
            var allGenres = getAllGenres();

            //returning them to the view so they can be displayed
            return View(allGenres);
        }

        // displaying the form for adding a new genre to the database
        [HttpGet]
        public IActionResult New()
        {
            return View(); 
        }

        // adding the new genre to the databse
        [HttpPost]
        public IActionResult New(Genre newGenre)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // adding the genre to the databse 
                    database.Add(newGenre);
                    database.SaveChanges();

                    TempData["message"] = "The genre " + newGenre.Name + " was added to the database!";

                    return Redirect("/Genres");
                }
                else
                {
                    return View(newGenre);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the genre: " + ex.Message);
                return View(newGenre);
            }
        }

        // private method to get all the genres from the database
        private IEnumerable<Genre> getAllGenres()
        {
            List<Genre> allGenres = database.Genres.ToList();
            return allGenres;
        }

        // private method to find genre based on the id 
        private Genre getGenreById(int id)
        {
            return database.Genres.FirstOrDefault(genre => genre.GenreId == id); 
        }

    }
}
