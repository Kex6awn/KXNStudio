using KxnPhotoStudio.Data;
using KxnPhotoStudio.Models;
using KxnPhotoStudio.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Services.Implementations
{
    public class ClientService : IClientService
    {
        private readonly AppDbContext _context;

        public ClientService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Client?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var normalizedEmail = email
                .Trim()
                .ToLowerInvariant();

            return await _context.Clients
                .FirstOrDefaultAsync(c => c.Email == normalizedEmail);
        }

        public async Task<Client?> GetByIdAsync(int clientId)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientId == clientId);
        }

        public async Task<Client> GetOrCreateClientAsync(Booking booking)
        {
            var normalizedEmail = booking.Email
                .Trim()
                .ToLowerInvariant();

            booking.Email = normalizedEmail;
            booking.FullName = booking.FullName.Trim();
            booking.PhoneNumber = booking.PhoneNumber?.Trim();

            var client = await GetByEmailAsync(normalizedEmail);

            if (client == null)
            {
                client = new Client
                {
                    FullName = booking.FullName,
                    Email = normalizedEmail,
                    PhoneNumber = booking.PhoneNumber,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                return client;
            }

            var clientChanged = false;

            if (client.FullName != booking.FullName)
            {
                client.FullName = booking.FullName;
                clientChanged = true;
            }

            if (!string.IsNullOrWhiteSpace(booking.PhoneNumber) &&
                client.PhoneNumber != booking.PhoneNumber)
            {
                client.PhoneNumber = booking.PhoneNumber;
                clientChanged = true;
            }

            if (clientChanged)
            {
                await _context.SaveChangesAsync();
            }

            return client;
        }
    }
}