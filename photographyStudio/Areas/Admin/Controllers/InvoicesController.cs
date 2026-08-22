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
        public async Task<IActionResult> Index(
            string? search,
            string? statusFilter)
        {
            var invoices = await _context.Invoices
                .Include(i => i.Booking)
                .ThenInclude(b => b.Client)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            // Overall totals before filtering
            ViewBag.TotalOutstanding =
                invoices.Sum(i => i.BalanceRemaining);

            ViewBag.PaidThisMonth =
                invoices
                    .Where(i =>
                        string.Equals(
                            i.Status,
                            "Paid",
                            StringComparison.OrdinalIgnoreCase)
                        &&
                        i.UpdatedAt.HasValue
                        &&
                        i.UpdatedAt.Value.Month ==
                            DateTime.UtcNow.Month
                        &&
                        i.UpdatedAt.Value.Year ==
                            DateTime.UtcNow.Year)
                    .Sum(i => i.AmountPaid);

            ViewBag.PendingCount =
                invoices.Count(i =>
                    string.Equals(
                        i.Status,
                        "Pending",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                        i.Status,
                        "Partially Paid",
                        StringComparison.OrdinalIgnoreCase));

            ViewBag.OverdueCount =
                invoices.Count(i =>
                    i.BalanceRemaining > 0 &&
                    i.DueDate.Date < DateTime.Today &&
                    !string.Equals(
                        i.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase));


            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim();

                invoices = invoices
                    .Where(i =>
                        i.InvoiceNumber.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        i.Booking.FullName.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        i.Booking.Email.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        i.Booking.ServiceType.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }


            // Status / business-state filter
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                invoices = statusFilter switch
                {
                    "Outstanding" => invoices
                        .Where(i =>
                            i.BalanceRemaining > 0 &&
                            !string.Equals(
                                i.Status,
                                "Cancelled",
                                StringComparison.OrdinalIgnoreCase))
                        .ToList(),

                    "Paid" => invoices
                        .Where(i =>
                            string.Equals(
                                i.Status,
                                "Paid",
                                StringComparison.OrdinalIgnoreCase))
                        .ToList(),

                    "Pending" => invoices
                        .Where(i =>
                            string.Equals(
                                i.Status,
                                "Pending",
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            string.Equals(
                                i.Status,
                                "Partially Paid",
                                StringComparison.OrdinalIgnoreCase))
                        .ToList(),

                    "Overdue" => invoices
                        .Where(i =>
                            i.BalanceRemaining > 0 &&
                            i.DueDate.Date < DateTime.Today &&
                            !string.Equals(
                                i.Status,
                                "Cancelled",
                                StringComparison.OrdinalIgnoreCase))
                        .ToList(),

                    "Cancelled" => invoices
                        .Where(i =>
                            string.Equals(
                                i.Status,
                                "Cancelled",
                                StringComparison.OrdinalIgnoreCase))
                        .ToList(),

                    _ => invoices
                };
            }

            ViewBag.Search = search;
            ViewBag.StatusFilter = statusFilter;

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

            if (!string.Equals(booking.Status, "Confirmed", StringComparison.OrdinalIgnoreCase))
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
                .Include(i => i.Payments)
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