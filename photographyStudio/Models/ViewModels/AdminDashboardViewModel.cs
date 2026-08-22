using KxnPhotoStudio.Models;

namespace KxnPhotoStudio.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Existing dashboard stats
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int DeclinedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int BookingsThisMonth { get; set; }
        public string? MostPopularService { get; set; }

        // Action Center
        public int OutstandingInvoicesCount { get; set; }
        public int NeedsEditingCount { get; set; }
        public int ReadyToDeliverCount { get; set; }

        public int TotalActionItems =>
            PendingBookings +
            OutstandingInvoicesCount +
            NeedsEditingCount +
            ReadyToDeliverCount;

        // Existing dashboard lists
        public List<Booking> RecentBookings { get; set; } = new();

        public List<Booking> UpcomingSessions { get; set; } = new();
    }
}