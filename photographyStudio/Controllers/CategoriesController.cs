using KxnPhotoStudio.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _kxnstudiodatabase;

        public CategoriesController(AppDbContext kxnstudiodatabase)
        {
            _kxnstudiodatabase = kxnstudiodatabase;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _kxnstudiodatabase.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }
    }
}