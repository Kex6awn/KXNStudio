namespace KxnPhotoStudio.Services.Interfaces
{
    public interface IWorkflowNotificationService
    {
        Task SendGalleryReadyEmailAsync(int bookingId);

        Task SendGalleryDeliveredEmailAsync(int bookingId);
    }
}