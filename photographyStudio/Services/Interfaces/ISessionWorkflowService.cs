using KxnPhotoStudio.Models;

namespace KxnPhotoStudio.Services.Interfaces
{
    public interface ISessionWorkflowService
    {
        Task<SessionWorkflow> GetOrCreateForBookingAsync(int bookingId);

        Task UpdateWorkflowAsync(
            int workflowId,
            string editingStatus,
            string deliveryStatus,
            string? galleryUrl,
            string? deliveryNotes);
    }
}