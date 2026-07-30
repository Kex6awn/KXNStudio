using KxnPhotoStudio.Models.ViewModels;

namespace KxnPhotoStudio.Services
{
    public interface IDashboardService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync();
    }
}