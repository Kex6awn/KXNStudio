using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using KxnPhotoStudio.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _context;

        public InvoiceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice> CreateInvoiceAsync(
            int bookingId,
            decimal subtotal,
            decimal tax,
            decimal discount,
            decimal depositRequired,
            DateTime dueDate,
            string? notes)
        {
            var booking = await _context.Bookings
                .Include(b => b.Invoice)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                throw new InvalidOperationException(
                    "The selected booking could not be found.");
            }

            if (booking.Invoice != null)
            {
                throw new InvalidOperationException(
                    "This booking already has an invoice.");
            }

            if (!string.Equals(
                    booking.Status,
                    "Confirmed",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "An invoice can only be created for a confirmed booking.");
            }

            if (subtotal < 0)
            {
                throw new InvalidOperationException(
                    "Subtotal cannot be negative.");
            }

            if (tax < 0)
            {
                throw new InvalidOperationException(
                    "Tax cannot be negative.");
            }

            if (discount < 0)
            {
                throw new InvalidOperationException(
                    "Discount cannot be negative.");
            }

            if (depositRequired < 0)
            {
                throw new InvalidOperationException(
                    "Deposit cannot be negative.");
            }

            if (dueDate.Date < DateTime.Today)
            {
                throw new InvalidOperationException(
                    "The due date cannot be in the past.");
            }

            var invoiceNumber = await GenerateInvoiceNumberAsync();

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                BookingId = booking.BookingId,
                IssueDate = DateTime.Today,
                DueDate = dueDate.Date,
                Subtotal = subtotal,
                Tax = tax,
                Discount = discount,
                DepositRequired = depositRequired,
                AmountPaid = 0,
                Status = "Pending",
                Notes = string.IsNullOrWhiteSpace(notes)
                    ? null
                    : notes.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            if (invoice.DepositRequired > invoice.Total)
            {
                throw new InvalidOperationException(
                    "The required deposit cannot exceed the invoice total.");
            }

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"INV-{year}-";

            var lastInvoiceNumber = await _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(i => i.InvoiceNumber)
                .Select(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            var nextSequence = 1;

            if (!string.IsNullOrWhiteSpace(lastInvoiceNumber))
            {
                var sequenceText =
                    lastInvoiceNumber.Substring(prefix.Length);

                if (int.TryParse(sequenceText, out var lastSequence))
                {
                    nextSequence = lastSequence + 1;
                }
            }

            return $"{prefix}{nextSequence:D4}";
        }
    }
}