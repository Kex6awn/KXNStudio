using KxnPhotoStudio.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly AppDbContext _context;

        public ClientsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients
                .Include(c => c.Bookings)
                .OrderBy(c => c.FullName)
                .ToListAsync();

            return View(clients);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Bookings)
                .Include(c => c.Notes)
                .FirstOrDefaultAsync(c => c.ClientId == id);

            if (client == null)
            {
                return NotFound();
            }

            client.Bookings = client.Bookings
                .OrderByDescending(b => b.EventDate)
                .ThenByDescending(b => b.StartTime)
                .ToList();

            client.Notes = client.Notes
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(client);
        }
    }
}
