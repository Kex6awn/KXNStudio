using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using KxnPhotoStudio.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;
        private readonly IClientService _clientService;

        public BookingService(
            AppDbContext context,
            IClientService clientService)
        {
            _context = context;
            _clientService = clientService;
        }

        public async Task<BookingCreationResult> CreateBookingAsync(
            Booking booking)
        {
            var businessStart = new TimeSpan(9, 0, 0);
            var businessEnd = new TimeSpan(18, 0, 0);

            if (booking.EventDate.Date < DateTime.Today)
            {
                return Fail(
                    "EventDate",
                    "Please select a future date.");
            }

            if (booking.StartTime < businessStart ||
                booking.StartTime >= businessEnd)
            {
                return Fail(
                    "StartTime",
                    "Bookings must be between 9:00 AM and 6:00 PM.");
            }

            if (booking.DurationHours < 1)
            {
                return Fail(
                    "DurationHours",
                    "Please select a valid duration.");
            }

            var requestedEndTime = booking.StartTime.Add(
                TimeSpan.FromHours(booking.DurationHours));

            if (requestedEndTime > businessEnd)
            {
                return Fail(
                    "DurationHours",
                    "This booking extends past business hours.");
            }

            var existingBookings = await _context.Bookings
                .Where(b =>
                    b.EventDate.Date == booking.EventDate.Date &&
                    b.Status != "Cancelled" &&
                    b.Status != "Declined")
                .ToListAsync();

            var overlaps = existingBookings.Any(existing =>
            {
                var existingStart = existing.StartTime;

                var existingEnd = existing.StartTime.Add(
                    TimeSpan.FromHours(existing.DurationHours));

                return booking.StartTime < existingEnd &&
                       requestedEndTime > existingStart;
            });

            if (overlaps)
            {
                return Fail(
                    string.Empty,
                    "That time slot is already booked. Please choose another time.");
            }

            booking.Status = "Pending";
            booking.CreatedAt = DateTime.UtcNow;

            var client = await _clientService
                .GetOrCreateClientAsync(booking);

            booking.ClientId = client.ClientId;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return new BookingCreationResult
            {
                Succeeded = true
            };
        }

        private static BookingCreationResult Fail(
            string errorField,
            string errorMessage)
        {
            return new BookingCreationResult
            {
                Succeeded = false,
                ErrorField = errorField,
                ErrorMessage = errorMessage
            };
        }
    }
}