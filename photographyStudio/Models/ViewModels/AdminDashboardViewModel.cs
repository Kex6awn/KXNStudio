using KxnPhotoStudio.Models;

namespace KxnPhotoStudio.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int DeclinedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int BookingsThisMonth { get; set; }
        public string? MostPopularService { get; set; }

        public List<Booking> RecentBookings { get; set; } = new();

        public List<Booking> UpcomingSessions { get; set; } = new();
    }
}
