using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using KxnPhotoStudio.Models.ViewModels;
using KxnPhotoStudio.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Services.Implementations
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

            var firstDayOfNextMonth =
                firstDayOfMonth.AddMonths(1);

            var mostPopularService = await _context.Bookings
                .Where(b =>
                    !string.IsNullOrWhiteSpace(b.ServiceType))
                .GroupBy(b => b.ServiceType)
                .Select(group => new
                {
                    ServiceName = group.Key,
                    BookingCount = group.Count()
                })
                .OrderByDescending(item =>
                    item.BookingCount)
                .Select(item =>
                    item.ServiceName)
                .FirstOrDefaultAsync();


            var outstandingInvoices = await _context.Invoices
                .Where(i => i.Status != "Cancelled")
                .ToListAsync();

            var outstandingInvoicesCount =
                outstandingInvoices.Count(i =>
                    i.BalanceRemaining > 0);


            var needsEditingCount =
                await _context.SessionWorkflows.CountAsync(w =>
                    w.EditingStatus == "Not Started");


            var readyToDeliverCount =
                await _context.SessionWorkflows.CountAsync(w =>
                    w.EditingStatus == "Completed" &&
                    w.DeliveryStatus == "Ready");


            var model = new AdminDashboardViewModel
            {
                TotalBookings =
                    await _context.Bookings.CountAsync(),

                PendingBookings =
                    await _context.Bookings.CountAsync(b =>
                        b.Status == BookingStatuses.Pending),

                ConfirmedBookings =
                    await _context.Bookings.CountAsync(b =>
                        b.Status == BookingStatuses.Confirmed),

                DeclinedBookings =
                    await _context.Bookings.CountAsync(b =>
                        b.Status == BookingStatuses.Declined),

                CancelledBookings =
                    await _context.Bookings.CountAsync(b =>
                        b.Status == BookingStatuses.Cancelled),

                BookingsThisMonth =
                    await _context.Bookings.CountAsync(b =>
                        b.CreatedAt >= firstDayOfMonth &&
                        b.CreatedAt < firstDayOfNextMonth),

                MostPopularService =
                    mostPopularService ?? "No bookings yet",


                // Action Center
                OutstandingInvoicesCount =
                    outstandingInvoicesCount,

                NeedsEditingCount =
                    needsEditingCount,

                ReadyToDeliverCount =
                    readyToDeliverCount,


                // Recent bookings
                RecentBookings =
                    await _context.Bookings
                        .OrderByDescending(b =>
                            b.CreatedAt)
                        .Take(5)
                        .ToListAsync(),


                // Upcoming confirmed sessions
                UpcomingSessions =
                    await _context.Bookings
                        .Where(b =>
                            b.Status ==
                                BookingStatuses.Confirmed &&
                            b.EventDate >= currentDate)
                        .OrderBy(b =>
                            b.EventDate)
                        .ThenBy(b =>
                            b.StartTime)
                        .Take(5)
                        .ToListAsync()
            };

            return model;
        }
    }
}