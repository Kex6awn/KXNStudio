using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models.ViewModels;
using KxnPhotoStudio.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class InvoicesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IInvoiceService _invoiceService;
        private readonly IPaymentService _paymentService;

        public InvoicesController(
            AppDbContext context,
            IInvoiceService invoiceService,
            IPaymentService paymentService)
        {
            _context = context;
            _invoiceService = invoiceService;
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Booking)
                .ThenInclude(b => b.Client)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(invoices);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Invoice)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Invoice != null)
            {
                TempData["WarningMessage"] =
                    "This booking already has an invoice.";

                return RedirectToAction(
                    "Details",
                    "Bookings",
                    new
                    {
                        area = "Admin",
                        id = bookingId
                    });
            }

            if (!string.Equals(
                    booking.Status,
                    "Confirmed",
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["WarningMessage"] =
                    "Only confirmed bookings can be invoiced.";

                return RedirectToAction(
                    "Details",
                    "Bookings",
                    new
                    {
                        area = "Admin",
                        id = bookingId
                    });
            }

            var model = new CreateInvoiceViewModel
            {
                BookingId = booking.BookingId,
                ClientName = booking.FullName,
                ServiceType = booking.ServiceType,
                EventDate = booking.EventDate,
                DueDate = DateTime.Today.AddDays(7)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateInvoiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var invoice = await _invoiceService.CreateInvoiceAsync(
                    model.BookingId,
                    model.Subtotal,
                    model.Tax,
                    model.Discount,
                    model.DepositRequired,
                    model.DueDate,
                    model.Notes);

                TempData["SuccessMessage"] =
                    $"Invoice {invoice.InvoiceNumber} was created successfully.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = invoice.InvoiceId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Booking)
                .ThenInclude(b => b.Client)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> RecordPayment(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Booking)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
            {
                return NotFound();
            }

            var model = new RecordPaymentViewModel
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                ClientName = invoice.Booking.FullName,
                Total = invoice.Total,
                AmountPaid = invoice.AmountPaid,
                BalanceRemaining = invoice.BalanceRemaining,
                PaymentDate = DateTime.Today,
                Method = "Cash"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(
            RecordPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _paymentService.RecordPaymentAsync(
                    model.InvoiceId,
                    model.Amount,
                    model.PaymentDate,
                    model.Method,
                    model.ReferenceNumber,
                    model.Notes);

                TempData["SuccessMessage"] =
                    "Payment recorded successfully.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.InvoiceId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
            }
        }
    }
}