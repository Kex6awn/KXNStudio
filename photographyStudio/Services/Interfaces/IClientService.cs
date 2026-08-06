using KxnPhotoStudio.Models;

namespace KxnPhotoStudio.Services.Interfaces
{
    public interface IClientService
    {
        Task<Client> GetOrCreateClientAsync(Booking booking);

        Task<Client?> GetByEmailAsync(string email);

        Task<Client?> GetByIdAsync(int clientId);
    }
}