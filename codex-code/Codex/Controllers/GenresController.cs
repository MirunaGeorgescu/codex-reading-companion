using Codex.Data;
using Codex.Migrations;
using Codex.Models;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Codex.Controllers
{
    [Authorize(Roles = "Editor,Admin")]
    public class GenresController : Controller
    {
        private readonly ApplicationDbContext database;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public GenresController(ApplicationDbContext context,
            UserManager<ApplicationUser> _userManager,
            RoleManager<IdentityRole> _roleManager)
        {
            database = context;
            userManager = _userManager;
            roleManager = _roleManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // getting all the genres from the database 
            var allGenres = getAllGenres();

            // returning them to the view so they can be displayed
            return View(allGenres);
        }

        // displaying the form for adding a new genre to the database
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult New()
        {
            return View(); 
        }

        // adding the new genre to the databse
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult New(Genre newGenre)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if(isGenreUnique(newGenre))
                    {
                        // adding the genre to the databse 
                        database.Add(newGenre);
                        database.SaveChanges();

                        TempData["message"] = "The genre " + newGenre.Name + " was added to the database!";

                        return Redirect("/Genres");
                    }
                    else
                    {
                        ModelState.AddModelError("Name", "The genre "+ newGenre.Name + " already exists in the database!");
                        return View(newGenre);
                    }
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
        [Authorize(Roles = "Editor,Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var genre = getGenreById(id);
            return View(genre);
        }

        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Genre newGenre)
        {
            // finding the original genre in the database
            var oldGenre = getGenreById(id);

            try
            {
                if (ModelState.IsValid)
                {
                    // making sure that the genre name is unique
                    if (isGenreUnique(newGenre))
                    {
                        // if the new genre is valid then we map the attributes to the old genre and save the changes 
                        mapGenreAttributes(ref oldGenre, newGenre);
                        database.SaveChanges();

                        TempData["message"] = "The genre " + oldGenre.Name + " was succesfully edited!";

                        // back to the index page so we can see the changes 
                        return Redirect("/Genres");
                    }
                    else
                    {
                        // if the genre is not unique then throw error
                        ModelState.AddModelError("Name", "The genre " + newGenre.Name + " already exists in the database!");
                        return View(newGenre);
                    }

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

        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public IActionResult Delete(int id) {
            // finding the genre in the database
            Genre genre = getGenreById(id);

            // removing the genre from the database
            database.Genres.Remove(genre);

            TempData["message"] = "The genre " + genre.Name + " was removed from the database!";

            // saving the changes made 
            database.SaveChanges();
            return RedirectToAction("Index");

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

        private void mapGenreAttributes(ref Genre destination, Genre source)
        {
            destination.Name = source.Name;
        }

        private bool isGenreUnique(Genre genre)
        {
            // trying to see if theres another genre with the same name in the database
            var genreWithSameName = getGenreByName(genre.Name);

            //if not then the genre is uinique, otherwise the genre is not unique
            return genreWithSameName == null; 
        }

        private Genre getGenreByName(string name)
        {
            // finding the genre with the given name in the database 
            return database.Genres.FirstOrDefault(genre => genre.Name == name); 
        }
    }
}
