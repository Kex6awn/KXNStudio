namespace KxnPhotoStudio.Services.Interfaces
{
    public interface IBookingStatusService
    {
        Task UpdateStatusAsync(int bookingId, string newStatus);

        IReadOnlyList<string> GetAllowedStatuses(string currentStatus);
    }
}
