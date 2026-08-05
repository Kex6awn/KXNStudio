namespace KxnPhotoStudio.Models.ViewModels
{
    public class BookingCreationResult
    {
        public bool Succeeded { get; set; }

        public string? ErrorField { get; set; }

        public string? ErrorMessage { get; set; }
    }
}