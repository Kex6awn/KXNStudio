using KxnPhotoStudio.Models;

namespace KxnPhotoStudio.Models.ViewModels
{
    public class BookingDetailsViewModel
    {
        public Booking Booking { get; set; } = null!;

        public JobCompletionResult JobCompletion { get; set; } = null!;

        public List<ClientNotification> Notifications { get; set; } = new();
    }
}