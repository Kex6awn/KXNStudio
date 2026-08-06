using KxnPhotoStudio.Models.ViewModels;

namespace KxnPhotoStudio.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync();
    }
}