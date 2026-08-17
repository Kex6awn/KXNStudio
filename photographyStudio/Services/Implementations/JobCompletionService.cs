using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using KxnPhotoStudio.Models.ViewModels;
using KxnPhotoStudio.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Services.Implementations
{
    public class JobCompletionService : IJobCompletionService
    {
        private readonly AppDbContext _context;

        public JobCompletionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<JobCompletionResult> GetJobCompletionAsync(
            int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Invoice)
                .Include(b => b.SessionWorkflow)
                .FirstOrDefaultAsync(b =>
                    b.BookingId == bookingId);

            if (booking == null)
            {
                throw new InvalidOperationException(
                    "The booking could not be found.");
            }

            var result = new JobCompletionResult
            {
                SessionCompleted =
                    string.Equals(
                        booking.Status,
                        BookingStatuses.Completed,
                        StringComparison.OrdinalIgnoreCase),

                InvoicePaid =
                    booking.Invoice != null &&
                    booking.Invoice.BalanceRemaining <= 0,

                GalleryDelivered =
                    booking.SessionWorkflow != null &&
                    string.Equals(
                        booking.SessionWorkflow.DeliveryStatus,
                        "Delivered",
                        StringComparison.OrdinalIgnoreCase)
            };

            return result;
        }
    }
}