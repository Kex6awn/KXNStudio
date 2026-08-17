using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KxnPhotoStudio.Models.ViewModels;
using KxnPhotoStudio.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KxnPhotoStudio.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IBookingStatusService _bookingStatusService;
        private readonly ISessionWorkflowService _sessionWorkflowService;
        private readonly IJobCompletionService _jobCompletionService;

        public BookingsController(
            AppDbContext context,
            IEmailService emailService,
            IBookingStatusService bookingStatusService,
            ISessionWorkflowService sessionWorkflowService,
            IJobCompletionService jobCompletionService)
        {
            _context = context;
            _emailService = emailService;
            _bookingStatusService = bookingStatusService;
            _sessionWorkflowService = sessionWorkflowService;
            _jobCompletionService = jobCompletionService;
        }

        // List all bookings
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        // View details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Invoice)
                .Include(b => b.SessionWorkflow)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            var jobCompletion = await _jobCompletionService.GetJobCompletionAsync(booking.BookingId);

            var model = new BookingDetailsViewModel
            {
                Booking = booking,
                JobCompletion = jobCompletion
            };

            return View(model);
        }

        // Edit status
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            var allowedStatuses =
                _bookingStatusService.GetAllowedStatuses(
                    booking.Status);

            var model = new BookingStatusViewModel
            {
                BookingId = booking.BookingId,
                CurrentStatus = booking.Status,
                Status = booking.Status,

                AllowedStatuses = allowedStatuses
                    .Select(status =>
                        new SelectListItem
                        {
                            Value = status,
                            Text = status
                        })
                    .ToList(),

                IsFinalStatus =
                    booking.Status == BookingStatuses.Completed ||
                    booking.Status == BookingStatuses.Declined ||
                    booking.Status == BookingStatuses.Cancelled
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetCalendarEvents()
        {
            var bookings = await _context.Bookings.Where(b => b.Status == "Confirmed" || b.Status == "Pending")
                                                    .Select(b => new {id = b.BookingId, title = b.ServiceType + " - " + b.FullName,
                                                    start = b.EventDate.Add(b.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
                                                    
                                                    end = b.EventDate.Add(b.StartTime).AddHours(b.DurationHours).ToString("yyyy-MM-ddTHH:mm:ss"),
                                                    
                                                    status = b.Status,
                                                    
                                                    url = Url.Action("Details", "Bookings",
                                                    new
                                                    {
                                                        area = "Admin",
                                                        id = b.BookingId
                                                    })
                                                }).ToListAsync();
            return Json(bookings);
        }

        // Calendar Action
        public IActionResult Calendar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookingStatusViewModel model)
        {
            if (id != model.BookingId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existing = await _context.Bookings.FindAsync(id);

            if (existing == null)
            {
                return NotFound();
            }

            var oldStatus = existing.Status;

            try
            {
                await _bookingStatusService.UpdateStatusAsync(
                    id,
                    model.Status);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    nameof(model.Status),
                    ex.Message);

                return View(model);
            }

            var statusChanged = !string.Equals(
                oldStatus,
                existing.Status,
                StringComparison.OrdinalIgnoreCase);

            if (statusChanged)
            {
                try
                {
                    await SendStatusEmailAsync(existing);

                    TempData["SuccessMessage"] =
                        $"Booking status changed to {existing.Status}. The customer was notified.";
                }
                catch (Exception)
                {
                    TempData["WarningMessage"] =
                        $"Booking status changed to {existing.Status}, but the email could not be sent.";
                }
            }
            else
            {
                TempData["SuccessMessage"] = "No status change was made.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task SendStatusEmailAsync(Booking booking)
        {
            if (string.IsNullOrWhiteSpace(booking.Email))
            {
                return;
            }

            var formattedDate = booking.EventDate.ToString("MMMM dd, yyyy");

            var formattedTime = DateTime.Today
                .Add(booking.StartTime)
                .ToString("h:mm tt");

            var normalizedStatus = booking.Status?
                .Trim()
                .ToLowerInvariant();

            string subject;
            string heading;
            string message;
            string statusText;
            string statusBackground;
            string statusColor;

            switch (normalizedStatus)
            {
                case "confirmed":
                    subject = "Your Booking Has Been Confirmed - KXN Photo Studio";
                    heading = "Your booking is confirmed!";
                    message =
                        "We’re pleased to confirm that your photography session has been accepted and reserved.";
                    statusText = "Confirmed";
                    statusBackground = "#d1e7dd";
                    statusColor = "#0f5132";
                    break;

                case "declined":
                    subject = "Booking Update - KXN Photo Studio";
                    heading = "An update about your booking";
                    message =
                        "Unfortunately, we are unable to approve your requested booking at this time.";
                    statusText = "Declined";
                    statusBackground = "#f8d7da";
                    statusColor = "#842029";
                    break;

                case "cancelled":
                    subject = "Your Booking Has Been Cancelled - KXN Photo Studio";
                    heading = "Your booking has been cancelled";
                    message =
                        "Your photography booking has been marked as cancelled.";
                    statusText = "Cancelled";
                    statusBackground = "#f8d7da";
                    statusColor = "#842029";
                    break;

                default:
                    subject = "Booking Status Updated - KXN Photo Studio";
                    heading = "Your booking status was updated";
                    message = $"Your booking status is now {booking.Status}.";
                    statusText = booking.Status ?? "Updated";
                    statusBackground = "#fff3cd";
                    statusColor = "#856404";
                    break;
            }

            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Booking Status Update</title>
                </head>

                <body style=""margin:0; padding:0; background-color:#f4f4f4; font-family:Arial, Helvetica, sans-serif;"">

                    <table role=""presentation"" width=""100%"" cellspacing=""0""
                           cellpadding=""0"" border=""0""
                           style=""background-color:#f4f4f4; padding:30px 15px;"">

                        <tr>
                            <td align=""center"">

                                <table role=""presentation"" width=""100%"" cellspacing=""0""
                                       cellpadding=""0"" border=""0""
                                       style=""max-width:650px; background-color:#ffffff;
                                              border-radius:12px; overflow:hidden;
                                              box-shadow:0 4px 18px rgba(0,0,0,0.08);"">

                                    <tr>
                                        <td style=""background-color:#111111;
                                                   padding:32px 25px;
                                                   text-align:center;"">

                                            <h1 style=""margin:0; color:#ffffff; font-size:28px;"">
                                                KXN Photo Studio
                                            </h1>

                                            <p style=""margin:10px 0 0;
                                                      color:#d6d6d6;
                                                      font-size:15px;"">
                                                Booking Status Update
                                            </p>

                                        </td>
                                    </tr>

                                    <tr>
                                        <td style=""padding:35px 30px;"">

                                            <h2 style=""margin:0 0 15px;
                                                       color:#222222;
                                                       font-size:23px;"">
                                                Hi {booking.FullName},
                                            </h2>

                                            <h3 style=""margin:0 0 15px;
                                                       color:#222222;
                                                       font-size:20px;"">
                                                {heading}
                                            </h3>

                                            <p style=""margin:0 0 28px;
                                                      color:#555555;
                                                      font-size:15px;
                                                      line-height:1.7;"">
                                                {message}
                                            </p>

                                            <table role=""presentation"" width=""100%""
                                                   cellspacing=""0"" cellpadding=""0"" border=""0""
                                                   style=""border-collapse:collapse;
                                                          border:1px solid #e6e6e6;"">

                                                <tr>
                                                    <td style=""padding:14px 16px;
                                                               background-color:#f8f8f8;
                                                               border-bottom:1px solid #e6e6e6;
                                                               font-weight:bold;
                                                               width:35%;"">
                                                        Service
                                                    </td>

                                                    <td style=""padding:14px 16px;
                                                               border-bottom:1px solid #e6e6e6;"">
                                                        {booking.ServiceType}
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td style=""padding:14px 16px;
                                                               background-color:#f8f8f8;
                                                               border-bottom:1px solid #e6e6e6;
                                                               font-weight:bold;"">
                                                        Date
                                                    </td>

                                                    <td style=""padding:14px 16px;
                                                               border-bottom:1px solid #e6e6e6;"">
                                                        {formattedDate}
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td style=""padding:14px 16px;
                                                               background-color:#f8f8f8;
                                                               border-bottom:1px solid #e6e6e6;
                                                               font-weight:bold;"">
                                                        Time
                                                    </td>

                                                    <td style=""padding:14px 16px;
                                                               border-bottom:1px solid #e6e6e6;"">
                                                        {formattedTime}
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td style=""padding:14px 16px;
                                                               background-color:#f8f8f8;
                                                               border-bottom:1px solid #e6e6e6;
                                                               font-weight:bold;"">
                                                        Duration
                                                    </td>

                                                    <td style=""padding:14px 16px;
                                                               border-bottom:1px solid #e6e6e6;"">
                                                        {booking.DurationHours} hour(s)
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td style=""padding:14px 16px;
                                                               background-color:#f8f8f8;
                                                               font-weight:bold;"">
                                                        Status
                                                    </td>

                                                    <td style=""padding:14px 16px;"">

                                                        <span style=""display:inline-block;
                                                                     background-color:{statusBackground};
                                                                     color:{statusColor};
                                                                     padding:6px 12px;
                                                                     border-radius:20px;
                                                                     font-size:13px;
                                                                     font-weight:bold;"">
                                                            {statusText}
                                                        </span>

                                                    </td>
                                                </tr>

                                            </table>

                                            <p style=""margin:28px 0 0;
                                                      color:#555555;
                                                      font-size:15px;
                                                      line-height:1.7;"">
                                                Please contact KXN Photo Studio if you have any
                                                questions about your booking.
                                            </p>

                                            <p style=""margin:22px 0 0;
                                                      color:#222222;
                                                      font-size:15px;
                                                      line-height:1.7;"">
                                                Sincerely,<br>
                                                <strong>KXN Photo Studio</strong>
                                            </p>

                                        </td>
                                    </tr>

                                    <tr>
                                        <td style=""background-color:#111111;
                                                   padding:22px;
                                                   text-align:center;"">

                                            <p style=""margin:0;
                                                      color:#bdbdbd;
                                                      font-size:13px;"">
                                                KXN Photo Studio
                                            </p>

                                        </td>
                                    </tr>

                                </table>

                            </td>
                        </tr>
                    </table>

                </body>
                </html>";

            await _emailService.SendEmailAsync(
                booking.Email,
                subject,
                body
            );
        }

        // Delete booking
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditWorkflow(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.SessionWorkflow)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            if (!string.Equals(
                    booking.Status,
                    BookingStatuses.Completed,
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["WarningMessage"] =
                    "Post-session workflow is only available for completed bookings.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = bookingId });
            }

            var workflow =
                booking.SessionWorkflow ??
                await _sessionWorkflowService
                    .GetOrCreateForBookingAsync(bookingId);

            var model = new SessionWorkflowViewModel
            {
                BookingId = booking.BookingId,
                SessionWorkflowId = workflow.SessionWorkflowId,
                EditingStatus = workflow.EditingStatus,
                DeliveryStatus = workflow.DeliveryStatus,
                GalleryUrl = workflow.GalleryUrl,
                DeliveryNotes = workflow.DeliveryNotes
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWorkflow(
            SessionWorkflowViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _sessionWorkflowService.UpdateWorkflowAsync(
                    model.SessionWorkflowId,
                    model.EditingStatus,
                    model.DeliveryStatus,
                    model.GalleryUrl,
                    model.DeliveryNotes);

                TempData["SuccessMessage"] =
                    "Post-session workflow updated successfully.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.BookingId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
            }
        }
    }
}