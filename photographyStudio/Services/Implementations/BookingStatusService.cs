using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using KxnPhotoStudio.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Services.Implementations
{
    public class BookingStatusService : IBookingStatusService
    {
        private readonly AppDbContext _context;
        private readonly ISessionWorkflowService _sessionWorkflowService;

        public BookingStatusService(AppDbContext context, ISessionWorkflowService sessionWorkflowService)
        {
            _context = context;
            _sessionWorkflowService = sessionWorkflowService;
        }

        public async Task UpdateStatusAsync(
            int bookingId,
            string newStatus)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                throw new InvalidOperationException(
                    "The booking could not be found.");
            }

            if (!BookingStatuses.All.Contains(newStatus))
            {
                throw new InvalidOperationException(
                    "The selected booking status is invalid.");
            }

            if (!IsValidTransition(
                    booking.Status,
                    newStatus))
            {
                throw new InvalidOperationException(
                    $"A booking cannot be changed from {booking.Status} to {newStatus}.");
            }

            booking.Status = newStatus;

            await _context.SaveChangesAsync();

            if (string.Equals(
                newStatus,
                BookingStatuses.Completed,
                StringComparison.OrdinalIgnoreCase))
            {
                await _sessionWorkflowService
                    .GetOrCreateForBookingAsync(booking.BookingId);
            }
        }

        private static bool IsValidTransition(
            string currentStatus,
            string newStatus)
        {
            if (string.Equals(
                    currentStatus,
                    newStatus,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return currentStatus switch
            {
                BookingStatuses.Pending =>
                    newStatus == BookingStatuses.Confirmed ||
                    newStatus == BookingStatuses.Declined ||
                    newStatus == BookingStatuses.Cancelled,

                BookingStatuses.Confirmed =>
                    newStatus == BookingStatuses.Completed ||
                    newStatus == BookingStatuses.Cancelled,

                BookingStatuses.Completed => false,

                BookingStatuses.Declined => false,

                BookingStatuses.Cancelled => false,

                _ => false
            };
        }

        public IReadOnlyList<string> GetAllowedStatuses(string currentStatus)
        {
            return currentStatus switch
            {
                BookingStatuses.Pending =>
                    new[]
                    {
                BookingStatuses.Pending,
                BookingStatuses.Confirmed,
                BookingStatuses.Declined,
                BookingStatuses.Cancelled
                    },

                BookingStatuses.Confirmed =>
                    new[]
                    {
                BookingStatuses.Confirmed,
                BookingStatuses.Completed,
                BookingStatuses.Cancelled
                    },

                BookingStatuses.Completed =>
                    new[]
                    {
                BookingStatuses.Completed
                    },

                BookingStatuses.Declined =>
                    new[]
                    {
                BookingStatuses.Declined
                    },

                BookingStatuses.Cancelled =>
                    new[]
                    {
                BookingStatuses.Cancelled
                    },

                _ => Array.Empty<string>()
            };
        }
    }
}