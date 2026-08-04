using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ClientNotesController : Controller
    {
        private readonly AppDbContext _context;

        public ClientNotesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Create
        public IActionResult Create(int clientId)
        {
            var note = new ClientNote
            {
                ClientId = clientId
            };

            return View(note);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientNote note)
        {
            if (!ModelState.IsValid)
            {
                return View(note);
            }

            note.CreatedAt = DateTime.UtcNow;

            _context.ClientNotes.Add(note);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Client note added successfully.";

            return RedirectToAction(
                "Details",
                "Clients",
                new
                {
                    area = "Admin",
                    id = note.ClientId
                });
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var note = await _context.ClientNotes.FindAsync(id);

            if (note == null)
                return NotFound();

            return View(note);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientNote note)
        {
            if (id != note.ClientNoteId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(note);

            var existing = await _context.ClientNotes.FindAsync(id);

            if (existing == null)
                return NotFound();

            existing.Content = note.Content;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Client note updated.";

            return RedirectToAction(
                "Details",
                "Clients",
                new
                {
                    area = "Admin",
                    id = existing.ClientId
                });
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var note = await _context.ClientNotes
                .Include(n => n.Client)
                .FirstOrDefaultAsync(n => n.ClientNoteId == id);

            if (note == null)
                return NotFound();

            return View(note);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var note = await _context.ClientNotes.FindAsync(id);

            if (note == null)
                return NotFound();

            var clientId = note.ClientId;

            _context.ClientNotes.Remove(note);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Client note deleted.";

            return RedirectToAction(
                "Details",
                "Clients",
                new
                {
                    area = "Admin",
                    id = clientId
                });
        }
    }
}