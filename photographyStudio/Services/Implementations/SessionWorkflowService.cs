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

        public async Task UpdateWorkflowAsync(
                int workflowId,
                string editingStatus,
                string deliveryStatus,
                string? galleryUrl,
                string? deliveryNotes)
            {
            var workflow = await _context.SessionWorkflows
                .FirstOrDefaultAsync(sw => sw.SessionWorkflowId == workflowId);

            if (workflow == null)
            {
                throw new InvalidOperationException("The session workflow could not be found.");
            }

            var validEditingStatuses = new[]
            {
                "Not Started",
                "In Progress",
                "Completed"
            };

            var validDeliveryStatuses = new[]
            {
                "Not Delivered",
                "Ready",
                "Delivered"
            };

            if (!validEditingStatuses.Contains(editingStatus))
            {
                throw new InvalidOperationException(
                    "The selected editing status is invalid.");
            }

            if (!validDeliveryStatuses.Contains(deliveryStatus))
            {
                throw new InvalidOperationException(
                    "The selected delivery status is invalid.");
            }

            if (deliveryStatus == "Ready" &&
                editingStatus != "Completed")
            {
                throw new InvalidOperationException(
                    "Editing must be completed before the gallery can be marked ready.");
            }

            if (deliveryStatus == "Delivered" &&
                editingStatus != "Completed")
            {
                throw new InvalidOperationException(
                    "Editing must be completed before the gallery can be delivered.");
            }

            workflow.EditingStatus = editingStatus;
            workflow.DeliveryStatus = deliveryStatus;

            workflow.GalleryUrl =
                string.IsNullOrWhiteSpace(galleryUrl)
                    ? null
                    : galleryUrl.Trim();

            workflow.DeliveryNotes =
                string.IsNullOrWhiteSpace(deliveryNotes)
                    ? null
                    : deliveryNotes.Trim();

            if (editingStatus == "In Progress" &&
                workflow.EditingStartedAt == null)
            {
                workflow.EditingStartedAt = DateTime.UtcNow;
            }

            if (editingStatus == "Completed" &&
                workflow.EditingCompletedAt == null)
            {
                workflow.EditingCompletedAt = DateTime.UtcNow;
            }

            if (deliveryStatus == "Delivered" &&
                workflow.DeliveredAt == null)
            {
                workflow.DeliveredAt = DateTime.UtcNow;
            }

            workflow.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}