using KxnPhotoStudio.Models;

namespace KxnPhotoStudio.Services.Interfaces
{
    public interface IBookingEmailService
    {
        Task SendNewBookingEmailsAsync(Booking booking);
    }
}
