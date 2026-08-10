using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using KxnPhotoStudio.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> RecordPaymentAsync(
            int invoiceId,
            decimal amount,
            DateTime paymentDate,
            string method,
            string? referenceNumber,
            string? notes)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
            {
                throw new InvalidOperationException(
                    "The invoice could not be found.");
            }

            if (amount <= 0)
            {
                throw new InvalidOperationException(
                    "Payment amount must be greater than zero.");
            }

            if (paymentDate.Date > DateTime.Today)
            {
                throw new InvalidOperationException(
                    "Payment date cannot be in the future.");
            }

            if (string.IsNullOrWhiteSpace(method))
            {
                throw new InvalidOperationException(
                    "Please select a payment method.");
            }

            if (string.Equals(
                    invoice.Status,
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Payments cannot be recorded against a cancelled invoice.");
            }

            var currentPaid = invoice.Payments.Sum(p => p.Amount);

            var remainingBalance =
                Math.Max(0, invoice.Total - currentPaid);

            if (amount > remainingBalance)
            {
                throw new InvalidOperationException(
                    $"Payment cannot exceed the remaining balance of {remainingBalance:C}.");
            }

            var payment = new Payment
            {
                InvoiceId = invoice.InvoiceId,
                Amount = amount,
                PaymentDate = paymentDate.Date,
                Method = method.Trim(),
                ReferenceNumber = string.IsNullOrWhiteSpace(referenceNumber)
                    ? null
                    : referenceNumber.Trim(),
                Notes = string.IsNullOrWhiteSpace(notes)
                    ? null
                    : notes.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            invoice.AmountPaid = currentPaid + amount;

            if (invoice.AmountPaid >= invoice.Total)
            {
                invoice.Status = "Paid";
            }
            else if (invoice.AmountPaid > 0)
            {
                invoice.Status = "Partially Paid";
            }
            else
            {
                invoice.Status = "Pending";
            }

            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return payment;
        }
    }
}