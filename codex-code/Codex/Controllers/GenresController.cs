using Codex.Data;
using Codex.Migrations;
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

        // displaying the form for editing a genre
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var genre = getGenreById(id);
            return View(genre);
        }

        [HttpPost]
        public IActionResult Edit(int id, Genre newGenre)
        {
            // finding the original genre in the database
            var oldGenre = getGenreById(id);

            try
            {
                if (ModelState.IsValid)
                {
                    // if the new genre is valid then we map the attributes to the old genre and save the changes 
                    MapAttributes(ref oldGenre, newGenre); 
                    database.SaveChanges();

                    TempData["message"] = "The genre " + oldGenre.Name + " was succesfully edited!";

                    // back to the index page so we can see the changes 
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

        private void MapAttributes(ref Genre destination, Genre source)
        {
            destination.Name = source.Name;
        }
    }
}
