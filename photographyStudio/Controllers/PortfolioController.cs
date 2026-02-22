using KxnPhotoStudio.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Controllers
{
    public class PortfolioController : Controller
    {

        private readonly AppDbContext _context;

        public PortfolioController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index ()
        {
            var photos = await _context.Photos.Include(p => p.Category).OrderByDescending(p => p.CreatedDate).ToListAsync();

            return View(photos);
        }
    }
}
