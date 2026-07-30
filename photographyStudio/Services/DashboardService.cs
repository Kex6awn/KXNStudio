using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardAsync()
        {
            var currentDate = DateTime.Today;

            var firstDayOfMonth = new DateTime(
                currentDate.Year,
                currentDate.Month,
                1);

            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var mostPopularService = await _context.Bookings
                .Where(b => !string.IsNullOrWhiteSpace(b.ServiceType))
                .GroupBy(b => b.ServiceType)
                .Select(group => new
                {
                    ServiceName = group.Key,
                    BookingCount = group.Count()
                })
                .OrderByDescending(item => item.BookingCount)
                .Select(item => item.ServiceName)
                .FirstOrDefaultAsync();

            var model = new AdminDashboardViewModel
            {
                TotalBookings = await _context.Bookings.CountAsync(),

                PendingBookings = await _context.Bookings.CountAsync(
                    b => b.Status == "Pending"),

                ConfirmedBookings = await _context.Bookings.CountAsync(
                    b => b.Status == "Confirmed"),

                DeclinedBookings = await _context.Bookings.CountAsync(
                    b => b.Status == "Declined"),

                CancelledBookings = await _context.Bookings.CountAsync(
                    b => b.Status == "Cancelled"),

                BookingsThisMonth = await _context.Bookings.CountAsync(
                    b => b.CreatedAt >= firstDayOfMonth &&
                         b.CreatedAt < firstDayOfNextMonth),

                MostPopularService =
                    mostPopularService ?? "No bookings yet",

                RecentBookings = await _context.Bookings
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                UpcomingSessions = await _context.Bookings
                    .Where(b =>
                        b.Status == "Confirmed" &&
                        b.EventDate >= currentDate)
                    .OrderBy(b => b.EventDate)
                    .ThenBy(b => b.StartTime)
                    .Take(5)
                    .ToListAsync()
            };

            return model;
        }
    }
}