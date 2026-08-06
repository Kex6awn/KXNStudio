using KxnPhotoStudio.Models;

namespace KxnPhotoStudio.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<Invoice> CreateInvoiceAsync(
            int bookingId,
            decimal subtotal,
            decimal tax,
            decimal discount,
            decimal depositRequired,
            DateTime dueDate,
            string? notes);
    }
}