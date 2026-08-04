using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int clientId, string content)
        {
            var clientExists = await _context.Clients.FindAsync(clientId);

            if (clientExists == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "The note cannot be empty.";

                return RedirectToAction(
                    "Details",
                    "Clients",
                    new { area = "Admin", id = clientId });
            }

            var note = new ClientNote
            {
                ClientId = clientId,
                Content = content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.ClientNotes.Add(note);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Client note added successfully.";

            return RedirectToAction(
                "Details",
                "Clients",
                new { area = "Admin", id = clientId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int clientNoteId,
            string content)
        {
            var note = await _context.ClientNotes.FindAsync(clientNoteId);

            if (note == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "The note cannot be empty.";

                return RedirectToAction(
                    "Details",
                    "Clients",
                    new { area = "Admin", id = note.ClientId });
            }

            note.Content = content.Trim();
            note.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Client note updated successfully.";

            return RedirectToAction(
                "Details",
                "Clients",
                new { area = "Admin", id = note.ClientId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int clientNoteId)
        {
            var note = await _context.ClientNotes.FindAsync(clientNoteId);

            if (note == null)
            {
                return NotFound();
            }

            var clientId = note.ClientId;

            _context.ClientNotes.Remove(note);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Client note deleted.";

            return RedirectToAction(
                "Details",
                "Clients",
                new { area = "Admin", id = clientId });
        }
    }
}