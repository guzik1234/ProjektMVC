using Microsoft.EntityFrameworkCore;
using Projekt_MVC.Models;

namespace Projekt_MVC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Advertisement> Advertisements { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja relacji Advertisement - User
            modelBuilder.Entity<Advertisement>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracja relacji wiele-do-wielu Advertisement - Category
            modelBuilder.Entity<Advertisement>()
                .HasMany(a => a.Categories)
                .WithMany(c => c.Advertisements)
                .UsingEntity(j => j.ToTable("AdvertisementCategories"));

            // Konfiguracja hierarchicznej struktury Category
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Dane przyk³adowe
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Email = "admin@example.com", PasswordHash = "hash123", IsAdmin = true },
                new User { Id = 2, Username = "jan_kowalski", Email = "jan@example.com", PasswordHash = "hash456", IsAdmin = false }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Motoryzacja", ParentCategoryId = null },
                new Category { Id = 2, Name = "Elektronika", ParentCategoryId = null },
                new Category { Id = 3, Name = "Samochody", ParentCategoryId = 1 },
                new Category { Id = 4, Name = "Telefony", ParentCategoryId = 2 }
            );

            modelBuilder.Entity<Advertisement>().HasData(
                new Advertisement { Id = 1, Title = "Sprzedam Opla", Description = "Bardzo sprawny samochód", Price = 5000, UserId = 1, CreatedAt = new DateTime(2025, 11, 29) },
                new Advertisement { Id = 2, Title = "Kupiê rower", Description = "Dowolny model, stan dobry", Price = 200, UserId = 2, CreatedAt = new DateTime(2025, 11, 29) }
            );
        }
    }
}
