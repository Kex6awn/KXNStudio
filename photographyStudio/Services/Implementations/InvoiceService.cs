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

        public async Task<Invoice> CreateInvoiceAsync(Booking booking)
        {
            throw new NotImplementedException();
        }
    }
}