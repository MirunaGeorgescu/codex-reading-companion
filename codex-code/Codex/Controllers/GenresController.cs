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
            var allGenres = getAllGenres();
            return View(allGenres);
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
