using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using KxnPhotoStudio.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Services.Implementations
{
    public class SessionWorkflowService : ISessionWorkflowService
    {
        private readonly AppDbContext _context;

        public SessionWorkflowService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SessionWorkflow> GetOrCreateForBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.SessionWorkflow)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                throw new InvalidOperationException(
                    "The booking could not be found.");
            }

            if (!string.Equals(
                    booking.Status,
                    BookingStatuses.Completed,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "A post-session workflow can only be created for a completed booking.");
            }

            if (booking.SessionWorkflow != null)
            {
                return booking.SessionWorkflow;
            }

            var workflow = new SessionWorkflow
            {
                BookingId = booking.BookingId,
                EditingStatus = "Not Started",
                DeliveryStatus = "Not Delivered",
                CreatedAt = DateTime.UtcNow
            };

            _context.SessionWorkflows.Add(workflow);

            await _context.SaveChangesAsync();

            return workflow;
        }
    }
}