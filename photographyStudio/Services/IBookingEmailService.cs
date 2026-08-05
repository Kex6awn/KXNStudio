using KxnPhotoStudio.Models;

namespace KxnPhotoStudio.Services
{
    public interface IBookingEmailService
    {
        Task SendNewBookingEmailsAsync(Booking booking);
    }
}
