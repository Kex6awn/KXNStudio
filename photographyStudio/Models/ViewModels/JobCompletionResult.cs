namespace KxnPhotoStudio.Models.ViewModels
{
    public class JobCompletionResult
    {
        public bool SessionCompleted { get; set; }

        public bool InvoicePaid { get; set; }

        public bool GalleryDelivered { get; set; }

        public bool IsJobComplete =>
            SessionCompleted &&
            InvoicePaid &&
            GalleryDelivered;
    }
}