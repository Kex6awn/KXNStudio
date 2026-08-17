using KxnPhotoStudio.Models.ViewModels;

namespace KxnPhotoStudio.Services.Interfaces
{
    public interface IJobCompletionService
    {
        Task<JobCompletionResult> GetJobCompletionAsync(
            int bookingId);
    }
}