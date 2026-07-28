using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KxnPhotoStudio.Services;

namespace KxnPhotoStudio.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public BookingController(AppDbContext context, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
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
                .Where(b => b.EventDate.Date >= startDate &&
                            b.EventDate.Date <= endDate &&
                            b.Status != "Cancelled")
                .ToListAsync();

            var fullyBookedDates = new List<string>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayBookings = bookings
                    .Where(b => b.EventDate.Date == date.Date)
                    .ToList();

                bool hasAvailableSlot = false;

                for (var slot = businessStart; slot < businessEnd; slot = slot.Add(TimeSpan.FromHours(1)))
                {
                    var requestedEnd = slot.Add(TimeSpan.FromHours(durationHours));

                    if (requestedEnd > businessEnd)
                        continue;

                    bool overlaps = dayBookings.Any(existing =>
                    {
                        var existingStart = existing.StartTime;
                        var existingEnd = existing.StartTime.Add(TimeSpan.FromHours(existing.DurationHours));

                        return slot < existingEnd && requestedEnd > existingStart;
                    });

                    if (!overlaps)
                    {
                        hasAvailableSlot = true;
                        break;
                    }
                }

                if (!hasAvailableSlot)
                {
                    fullyBookedDates.Add(date.ToString("yyyy-MM-dd"));
                }
            }

            return Json(fullyBookedDates);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(DateTime eventDate, int durationHours)
        {
            var businessStart = new TimeSpan(9, 0, 0);   // 9:00 AM
            var businessEnd = new TimeSpan(18, 0, 0);    // 6:00 PM

            if (durationHours < 1)
            {
                return Json(new List<string>());
            }

            var existingBookings = await _context.Bookings
                .Where(b => b.EventDate.Date == eventDate.Date && b.Status != "Cancelled")
                .ToListAsync();

            var availableSlots = new List<string>();

            for (var slot = businessStart; slot < businessEnd; slot = slot.Add(TimeSpan.FromHours(1)))
            {
                var requestedEnd = slot.Add(TimeSpan.FromHours(durationHours));

                if (requestedEnd > businessEnd)
                    continue;

                bool overlaps = existingBookings.Any(existing =>
                {
                    var existingStart = existing.StartTime;
                    var existingEnd = existing.StartTime.Add(TimeSpan.FromHours(existing.DurationHours));

                    return slot < existingEnd && requestedEnd > existingStart;
                });

                if (!overlaps)
                {
                    availableSlots.Add(slot.ToString(@"hh\:mm"));
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
            if (booking.EventDate.Date < DateTime.Today)
            {
                ModelState.AddModelError("EventDate", "Please select a future date.");
            }

            var businessStart = new TimeSpan(9, 0, 0); //9:00 AM
            var businessEnd = new TimeSpan(18, 0, 0);  //6:00 AM

            if (booking.StartTime < businessStart || booking.StartTime >= businessEnd)
            {
                ModelState.AddModelError("StartTime", "Bookings must be between 9:00 AM and 6:00 PM.");
            }

            var requestedEndTime = booking.StartTime.Add(TimeSpan.FromHours(booking.DurationHours));

            if (requestedEndTime > businessEnd)
            {
                ModelState.AddModelError("DurationHours", "This booking extends past business hours.");
            }

            var existingBookings = await _context.Bookings
                                    .Where(b => b.EventDate.Date == booking.EventDate.Date && b.Status != "Cancelled")
                                    .ToListAsync();

            foreach (var existing in existingBookings)
            {
                var existingStart = existing.StartTime;
                var existingEnd = existing.StartTime.Add(TimeSpan.FromHours(existing.DurationHours));

                bool overlaps = booking.StartTime < existingEnd && requestedEndTime > existingStart;

                if (overlaps)
                {
                    ModelState.AddModelError(string.Empty, "That time slot is already booked. Please choose another time.");
                    break;
                }
            }

            if (!ModelState.IsValid)
            {
                return View(booking);
            }

            booking.Status = "Pending";
            booking.CreatedAt = DateTime.UtcNow;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var adminEmail = _configuration["EmailSettings:AdminEmail"];

            var subject = "New Booking Request - KXN Photo Studio";

            var formattedDate = booking.EventDate.ToString("MMMM dd, yyyy");
            var formattedTime = DateTime.Today
                .Add(booking.StartTime)
                .ToString("h:mm tt");

            var messageText = string.IsNullOrWhiteSpace(booking.Message)
                ? "No additional message was provided."
                : booking.Message;

            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>New Booking Request</title>
                </head>

                <body style=""margin:0; padding:0; background-color:#f4f4f4; font-family:Arial, Helvetica, sans-serif;"">

                    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0""
                           style=""background-color:#f4f4f4; padding:30px 15px;"">
                        <tr>
                            <td align=""center"">

                                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0""
                                       style=""max-width:650px; background-color:#ffffff; border-radius:12px; overflow:hidden;
                                              box-shadow:0 4px 18px rgba(0,0,0,0.08);"">

                                    <!-- Header -->
                                    <tr>
                                        <td style=""background-color:#111111; padding:32px 25px; text-align:center;"">
                                            <h1 style=""margin:0; color:#ffffff; font-size:28px; letter-spacing:1px;"">
                                                KXN Photo Studio
                                            </h1>

                                            <p style=""margin:10px 0 0; color:#d6d6d6; font-size:15px;"">
                                                New Booking Request
                                            </p>
                                        </td>
                                    </tr>

                                    <!-- Main Content -->
                                    <tr>
                                        <td style=""padding:35px 30px;"">

                                            <h2 style=""margin:0 0 10px; color:#222222; font-size:23px;"">
                                                You received a new booking
                                            </h2>

                                            <p style=""margin:0 0 28px; color:#666666; font-size:15px; line-height:1.6;"">
                                                A customer submitted a booking request through the KXN Photo Studio website.
                                            </p>

                                            <!-- Booking Details -->
                                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0""
                                                   style=""border-collapse:collapse; border:1px solid #e6e6e6; border-radius:8px;"">

                                                <tr>
                                                    <td style=""padding:14px 16px; background-color:#f8f8f8;
                                                               border-bottom:1px solid #e6e6e6; font-weight:bold; width:35%;"">
                                                        Customer
                                                    </td>
                                                    <td style=""padding:14px 16px; border-bottom:1px solid #e6e6e6;"">
                                                        {booking.FullName}
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td style=""padding:14px 16px; background-color:#f8f8f8;
                                                               border-bottom:1px solid #e6e6e6; font-weight:bold;"">
                                                        Email
                                                    </td>
                                                    <td style=""padding:14px 16px; border-bottom:1px solid #e6e6e6;"">
                                                        <a href=""mailto:{booking.Email}""
                                                           style=""color:#111111; text-decoration:none;"">
                                                            {booking.Email}
                                                        </a>
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td style=""padding:14px 16px; background-color:#f8f8f8;
                                                               border-bottom:1px solid #e6e6e6; font-weight:bold;"">
                                                        Phone
                                                    </td>
                                                    <td style=""padding:14px 16px; border-bottom:1px solid #e6e6e6;"">
                                                        <a href=""tel:{booking.PhoneNumber}""
                                                           style=""color:#111111; text-decoration:none;"">
                                                            {booking.PhoneNumber}
                                                        </a>
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td style=""padding:14px 16px; background-color:#f8f8f8;
                                                               border-bottom:1px solid #e6e6e6; font-weight:bold;"">
                                                        Service
                                                    </td>
                                                    <td style=""padding:14px 16px; border-bottom:1px solid #e6e6e6;"">
                                                        {booking.ServiceType}
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td style=""padding:14px 16px; background-color:#f8f8f8;
                                                               border-bottom:1px solid #e6e6e6; font-weight:bold;"">
                                                        Date
                                                    </td>
                                                    <td style=""padding:14px 16px; border-bottom:1px solid #e6e6e6;"">
                                                        {formattedDate}
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td style=""padding:14px 16px; background-color:#f8f8f8;
                                                               border-bottom:1px solid #e6e6e6; font-weight:bold;"">
                                                        Time
                                                    </td>
                                                    <td style=""padding:14px 16px; border-bottom:1px solid #e6e6e6;"">
                                                        {formattedTime}
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td style=""padding:14px 16px; background-color:#f8f8f8;
                                                               border-bottom:1px solid #e6e6e6; font-weight:bold;"">
                                                        Duration
                                                    </td>
                                                    <td style=""padding:14px 16px; border-bottom:1px solid #e6e6e6;"">
                                                        {booking.DurationHours} hour(s)
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td style=""padding:14px 16px; background-color:#f8f8f8;
                                                               font-weight:bold;"">
                                                        Status
                                                    </td>
                                                    <td style=""padding:14px 16px;"">
                                                        <span style=""display:inline-block; background-color:#fff3cd;
                                                                     color:#856404; padding:6px 12px; border-radius:20px;
                                                                     font-size:13px; font-weight:bold;"">
                                                            {booking.Status}
                                                        </span>
                                                    </td>
                                                </tr>

                                            </table>

                                            <!-- Customer Message -->
                                            <div style=""margin-top:28px;"">
                                                <h3 style=""margin:0 0 10px; color:#222222; font-size:18px;"">
                                                    Customer Message
                                                </h3>

                                                <div style=""background-color:#f8f8f8; border-left:4px solid #111111;
                                                            padding:16px; color:#555555; line-height:1.6;"">
                                                    {messageText}
                                                </div>
                                            </div>

                                            <p style=""margin:28px 0 0; color:#666666; font-size:14px; line-height:1.6;"">
                                                Log in to the KXN Photo Studio admin dashboard to review and manage this booking.
                                            </p>

                                        </td>
                                    </tr>

                                    <!-- Footer -->
                                    <tr>
                                        <td style=""background-color:#111111; padding:22px; text-align:center;"">
                                            <p style=""margin:0; color:#bdbdbd; font-size:13px;"">
                                                KXN Photo Studio Booking Notification
                                            </p>
                                        </td>
                                    </tr>

                                </table>

                            </td>
                        </tr>
                    </table>

                </body>
                </html>";

            try
            {
                if (!string.IsNullOrEmpty(adminEmail))
                {
                    await _emailService.SendEmailAsync(adminEmail, subject, body);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Email failed: " + ex.Message, ex);
            }

            return RedirectToAction(nameof(ThankYou));
        }

        public IActionResult ThankYou()
        {
            return View();
        }
    }
}