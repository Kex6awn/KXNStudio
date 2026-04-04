using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using Microsoft.AspNetCore.Mvc;

namespace KxnPhotoStudio.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                return View(booking);
            }

            booking.Status = "Pending";
            booking.CreatedAt = DateTime.UtcNow;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ThankYou));
        }

        public IActionResult ThankYou()
        {
            return View();
        }
    }
}