using KxnPhotoStudio.Models;
using KxnPhotoStudio.Models.ViewModels;
using KxnPhotoStudio.Services.Interfaces;

namespace KxnPhotoStudio.Services.Implementations
{
    public class BookingEmailService : IBookingEmailService
    {
        private readonly IEmailService _emailService;
        private readonly IRazorViewRenderService _razorViewRenderService;
        private readonly IConfiguration _configuration;

        public BookingEmailService(
            IEmailService emailService,
            IRazorViewRenderService razorViewRenderService,
            IConfiguration configuration)
        {
            _emailService = emailService;
            _razorViewRenderService = razorViewRenderService;
            _configuration = configuration;
        }

        public async Task SendNewBookingEmailsAsync(Booking booking)
        {
            var model = new BookingEmailViewModel
            {
                FullName = booking.FullName,
                Email = booking.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(booking.PhoneNumber)
                    ? "Not provided"
                    : booking.PhoneNumber,
                ServiceType = booking.ServiceType,
                FormattedDate = booking.EventDate
                    .ToString("MMMM dd, yyyy"),
                FormattedTime = DateTime.Today
                    .Add(booking.StartTime)
                    .ToString("h:mm tt"),
                DurationHours = booking.DurationHours,
                Status = booking.Status,
                MessageText = string.IsNullOrWhiteSpace(booking.Message)
                    ? "No additional message was provided."
                    : booking.Message
            };

            var adminBody =
                await _razorViewRenderService.RenderViewToStringAsync(
                    "EmailTemplates/AdminNewBooking",
                    model);

            var customerBody =
                await _razorViewRenderService.RenderViewToStringAsync(
                    "EmailTemplates/CustomerBookingReceived",
                    model);

            var adminEmail =
                _configuration["EmailSettings:AdminEmail"];

            if (!string.IsNullOrWhiteSpace(adminEmail))
            {
                await _emailService.SendEmailAsync(
                    adminEmail,
                    "New Booking Request - KXN Photo Studio",
                    adminBody);
            }

            if (!string.IsNullOrWhiteSpace(booking.Email))
            {
                await _emailService.SendEmailAsync(
                    booking.Email,
                    "We Received Your Booking Request - KXN Photo Studio",
                    customerBody);
            }
        }
    }
}