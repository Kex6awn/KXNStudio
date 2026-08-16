using KxnPhotoStudio.Models;

namespace KxnPhotoStudio.Services.Interfaces
{
    public interface ISessionWorkflowService
    {
        Task<SessionWorkflow> GetOrCreateForBookingAsync(int bookingId);
    }
}