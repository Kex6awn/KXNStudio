//using KxnPhotoStudio.Models;
//using Microsoft.EntityFrameworkCore;

//namespace KxnPhotoStudio.Data
//{
//    public static class ClientDataSeeder
//    {
//        public static async Task BackfillClientsAsync(
//            AppDbContext context)
//        {
//            var bookingsWithoutClients = await context.Bookings
//                .Where(b => b.ClientId == null)
//                .OrderBy(b => b.CreatedAt)
//                .ToListAsync();

//            foreach (var booking in bookingsWithoutClients)
//            {
//                var normalizedEmail = booking.Email
//                    .Trim()
//                    .ToLower();

//                var client = await context.Clients
//                    .FirstOrDefaultAsync(c =>
//                        c.Email.ToLower() == normalizedEmail);

//                if (client == null)
//                {
//                    client = new Client
//                    {
//                        FullName = booking.FullName,
//                        Email = booking.Email.Trim(),
//                        PhoneNumber = booking.PhoneNumber,
//                        CreatedAt = booking.CreatedAt
//                    };

//                    context.Clients.Add(client);

//                    // Generating ClientId before assigning it to Booking.
//                    await context.SaveChangesAsync();
//                }

//                booking.ClientId = client.ClientId;
//            }

//            await context.SaveChangesAsync();
//        }
//    }
//}