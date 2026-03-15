using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using KxnPhotoStudio.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PhotosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PhotosController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var photos = await _context.Photos
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return View(photos);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhotoUploadViewModel model)
        {
            if (model.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Please select an image.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", model.CategoryId);
                return View(model);
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var safeFileName = Path.GetFileName(model.ImageFile.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            var photo = new Photo
            {
                Title = model.Title,
                Description = model.Description,
                CategoryId = model.CategoryId,
                ImagePath = "/uploads/" + uniqueFileName,
                CreatedDate = DateTime.UtcNow
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var photo = await _context.Photos.FindAsync(id);
            if (photo == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", photo.CategoryId);

            var model = new PhotoUploadViewModel
            {
                Title = photo.Title,
                Description = photo.Description,
                CategoryId = photo.CategoryId
            };

            ViewBag.PhotoId = photo.PhotoId;
            ViewBag.CurrentImagePath = photo.ImagePath;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PhotoUploadViewModel model)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", model.CategoryId);
                ViewBag.PhotoId = id;
                ViewBag.CurrentImagePath = photo.ImagePath;
                return View(model);
            }

            photo.Title = model.Title;
            photo.Description = model.Description;
            photo.CategoryId = model.CategoryId;

            if (model.ImageFile != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var safeFileName = Path.GetFileName(model.ImageFile.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(photo.ImagePath))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, photo.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                photo.ImagePath = "/uploads/" + uniqueFileName;
            }

            _context.Update(photo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var photo = await _context.Photos
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.PhotoId == id);

            if (photo == null) return NotFound();

            return View(photo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo != null)
            {
                if (!string.IsNullOrEmpty(photo.ImagePath))
                {
                    var fullPath = Path.Combine(_env.WebRootPath, photo.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                _context.Photos.Remove(photo);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}