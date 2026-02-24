using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using KxnPhotoStudio.Models;


namespace KxnPhotoStudio.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Photo> Photos => Set<Photo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: seed starter categories

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Potraits"},
                new Category { CategoryId = 2, Name = "Weddings"},
                new Category { CategoryId = 3, Name = "Events"},
                new Category { CategoryId = 4, Name = "Outdoor"}
            );
        }
    }
}
