namespace KxnPhotoStudio.Models.ViewModels
{
    public class BookingEmailViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string FormattedDate { get; set; } = string.Empty;
        public string FormattedTime { get; set; } = string.Empty;
        public int DurationHours { get; set; }
        public string Status { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
    }
}