using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using KxnPhotoStudio.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IBookingService _bookingService;
        private readonly IBookingEmailService _bookingEmailService;

        public BookingController(
            AppDbContext context,
            IBookingService bookingService,
            IBookingEmailService bookingEmailService)
        {
            _context = context;
            _bookingService = bookingService;
            _bookingEmailService = bookingEmailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFullyBookedDates(int durationHours)
        {
            var businessStart = new TimeSpan(9, 0, 0);
            var businessEnd = new TimeSpan(18, 0, 0);

            if (durationHours < 1)
            {
                return Json(new List<string>());
            }

            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddMonths(3);

            var bookings = await _context.Bookings
                .Where(b =>
                    b.EventDate.Date >= startDate &&
                    b.EventDate.Date <= endDate &&
                    b.Status != "Cancelled" &&
                    b.Status != "Declined")
                .ToListAsync();

            var fullyBookedDates = new List<string>();

            for (var date = startDate;
                 date <= endDate;
                 date = date.AddDays(1))
            {
                var dayBookings = bookings
                    .Where(b => b.EventDate.Date == date.Date)
                    .ToList();

                var hasAvailableSlot = false;

                for (var slot = businessStart;
                     slot < businessEnd;
                     slot = slot.Add(TimeSpan.FromHours(1)))
                {
                    var requestedEnd = slot.Add(
                        TimeSpan.FromHours(durationHours));

                    if (requestedEnd > businessEnd)
                    {
                        continue;
                    }

                    var overlaps = dayBookings.Any(existing =>
                    {
                        var existingStart = existing.StartTime;

                        var existingEnd = existing.StartTime.Add(
                            TimeSpan.FromHours(
                                existing.DurationHours));

                        return slot < existingEnd &&
                               requestedEnd > existingStart;
                    });

                    if (!overlaps)
                    {
                        hasAvailableSlot = true;
                        break;
                    }
                }

                if (!hasAvailableSlot)
                {
                    fullyBookedDates.Add(
                        date.ToString("yyyy-MM-dd"));
                }
            }

            return Json(fullyBookedDates);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(
            DateTime eventDate,
            int durationHours)
        {
            var businessStart = new TimeSpan(9, 0, 0);
            var businessEnd = new TimeSpan(18, 0, 0);

            if (durationHours < 1)
            {
                return Json(new List<string>());
            }

            var existingBookings = await _context.Bookings
                .Where(b =>
                    b.EventDate.Date == eventDate.Date &&
                    b.Status != "Cancelled" &&
                    b.Status != "Declined")
                .ToListAsync();

            var availableSlots = new List<string>();

            for (var slot = businessStart;
                 slot < businessEnd;
                 slot = slot.Add(TimeSpan.FromHours(1)))
            {
                var requestedEnd = slot.Add(
                    TimeSpan.FromHours(durationHours));

                if (requestedEnd > businessEnd)
                {
                    continue;
                }

                var overlaps = existingBookings.Any(existing =>
                {
                    var existingStart = existing.StartTime;

                    var existingEnd = existing.StartTime.Add(
                        TimeSpan.FromHours(
                            existing.DurationHours));

                    return slot < existingEnd &&
                           requestedEnd > existingStart;
                });

                if (!overlaps)
                {
                    availableSlots.Add(
                        slot.ToString(@"hh\:mm"));
                }
            }

            return Json(availableSlots);
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

            var result =
                await _bookingService.CreateBookingAsync(booking);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    result.ErrorField ?? string.Empty,
                    result.ErrorMessage ??
                    "The booking could not be created.");

                return View(booking);
            }

            try
            {
                await _bookingEmailService
                    .SendNewBookingEmailsAsync(booking);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Booking email failed: {ex.Message}");

                TempData["WarningMessage"] =
                    "Your booking was saved, but the confirmation email could not be sent.";
            }

            return RedirectToAction(nameof(ThankYou));
        }

        public IActionResult ThankYou()
        {
            return View();
        }
    }
}