using KxnPhotoStudio.Models;
using KxnPhotoStudio.Models.ViewModels;

namespace KxnPhotoStudio.Services
{
    public interface IBookingService
    {
        Task<BookingCreationResult> CreateBookingAsync(Booking booking);
    }
}