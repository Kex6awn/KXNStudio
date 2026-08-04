using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using KxnPhotoStudio.Models;


namespace KxnPhotoStudio.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Photo> Photos => Set<Photo>();

        public DbSet<Client> Clients => Set<Client>();

        public DbSet<Booking> Bookings => Set<Booking>();

        public DbSet<ClientNote> ClientNotes => Set<ClientNote>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>().HasIndex(c => c.Email).IsUnique();

            modelBuilder.Entity<Client>()
                .HasMany(c => c.Bookings)
                .WithOne(b => b.Client)
                .HasForeignKey(b => b.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Client>()
                .HasMany(client => client.ClientNotes)
                .WithOne(note => note.Client)
                .HasForeignKey(note => note.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // seed starter categories

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Potraits"},
                new Category { CategoryId = 2, Name = "Weddings"},
                new Category { CategoryId = 3, Name = "Events"},
                new Category { CategoryId = 4, Name = "Outdoor"}
            );
        }
    }
}
