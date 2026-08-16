namespace KxnPhotoStudio.Models
{
    public static class BookingStatuses
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string Completed = "Completed";
        public const string Declined = "Declined";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All =
        {
            Pending,
            Confirmed,
            Completed,
            Declined,
            Cancelled
        };
    }
}