using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using KxnPhotoStudio.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // GET: Upload
        public async Task<IActionResult> Upload()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(PhotoUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
                return View(model);
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(fileStream);
            }

            var photo = new Photo
            {
                Title = model.Title,
                Description = model.Description,
                CategoryId = model.CategoryId,
                ImagePath = "/uploads/" + uniqueFileName
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
