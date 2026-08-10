using KxnPhotoStudio.Models;

namespace KxnPhotoStudio.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> RecordPaymentAsync(
            int invoiceId,
            decimal amount,
            DateTime paymentDate,
            string method,
            string? referenceNumber,
            string? notes);
    }
}