using KxnPhotoStudio.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredPhotos = await _context.Photos
                .Include(p => p.Category)
                .Where(p => p.IsFeatured)
                .OrderByDescending(p => p.CreatedDate)
                .Take(6)
                .ToListAsync();

            return View(featuredPhotos);
        }
    }
}
