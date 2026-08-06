using KxnPhotoStudio.Models;

namespace KxnPhotoStudio.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<Invoice> CreateInvoiceAsync(Booking booking);
    }
}