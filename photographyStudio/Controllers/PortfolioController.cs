using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using KxnPhotoStudio.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Authorization;
//using KxnPhotoStudio.Services;

namespace KxnPhotoStudio.Controllers
{
    public class PortfolioController : Controller
    {

        private readonly AppDbContext _context;

        public PortfolioController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.SelectedCategoryId = categoryId;

            var query = _context.Photos.Include(p => p.Category).AsQueryable();
            
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var photos = await query
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return View(photos);
        }

        // GET: Upload
        //[Authorize]
        //public IActionResult Upload()
        //{
        //    ViewBag.Categories = new SelectList(
        //        _context.Categories,
        //        "CategoryId",
        //        "Name");

        //    return View();
        //}

        // POST: Upload
        //[HttpPost]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Upload(PhotoUploadViewModel model)
        //{
        //    var imageFile = model.ImageFile;

        //    if (imageFile == null)
        //    {
        //        ModelState.AddModelError(
        //            "ImageFile",
        //            "Please select an image.");

        //        ViewBag.Categories = new SelectList(
        //            _context.Categories,
        //            "CategoryId",
        //            "Name");

        //        return View(model);
        //    }

        //    var validationError = await ImageUploadValidator.ValidateAsync(imageFile);

        //    if (validationError != null)
        //    {
        //        ModelState.AddModelError(
        //            "ImageFile",
        //            validationError);

        //        ViewBag.Categories = new SelectList(
        //            _context.Categories,
        //            "CategoryId",
        //            "Name");

        //        return View(model);
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.Categories = new SelectList(
        //            _context.Categories,
        //            "CategoryId",
        //            "Name");

        //        return View(model);
        //    }

        //    var uploadsFolder = Path.Combine(
        //        Directory.GetCurrentDirectory(),
        //        "wwwroot/uploads");

        //    if (!Directory.Exists(uploadsFolder))
        //        Directory.CreateDirectory(uploadsFolder);

        //    var safeFileName = Path.GetFileName(imageFile.FileName);

        //    var uniqueFileName =
        //        $"{Guid.NewGuid()}_{safeFileName}";

        //    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //    using (var fileStream = new FileStream(filePath, FileMode.Create))
        //    {
        //        await imageFile.CopyToAsync(fileStream);
        //    }

        //    var photo = new Photo
        //    {
        //        Title = model.Title,
        //        Description = model.Description,
        //        CategoryId = model.CategoryId,
        //        ImagePath = "/uploads/" + uniqueFileName
        //    };

        //    _context.Photos.Add(photo);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction("Index");
        //}
    }
}
