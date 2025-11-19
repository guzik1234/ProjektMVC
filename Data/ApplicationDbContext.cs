using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ogloszenia.Models;

namespace ogloszenia.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Custom entities
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Advertisement> Advertisements { get; set; }
        public DbSet<AdvertisementAttribute> AdvertisementAttributes { get; set; }
        public DbSet<AdvertisementAttributeValue> AttributeValues { get; set; }
        public DbSet<Dictionary> Dictionaries { get; set; }
        public DbSet<DictionaryValue> DictionaryValues { get; set; }
        public DbSet<AdvertisementMedia> Media { get; set; }
        public DbSet<AdvertisementFile> Files { get; set; }
        public DbSet<ModerationReport> ModerationReports { get; set; }
        public DbSet<NewsletterCriteria> NewsletterCriteria { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<RssConfig> RssConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints
            modelBuilder.Entity<Category>()
                .HasMany(c => c.ChildCategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Advertisements)
                .WithMany(a => a.Categories);

            modelBuilder.Entity<Advertisement>()
                .HasOne(a => a.User)
                .WithMany(u => u.Advertisements)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdvertisementAttributeValue>()
                .HasOne(av => av.Advertisement)
                .WithMany(a => a.AttributeValues)
                .HasForeignKey(av => av.AdvertisementId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdvertisementMedia>()
                .HasOne(m => m.Advertisement)
                .WithMany(a => a.Media)
                .HasForeignKey(m => m.AdvertisementId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdvertisementFile>()
                .HasOne(f => f.Advertisement)
                .WithMany(a => a.Files)
                .HasForeignKey(f => f.AdvertisementId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ModerationReport>()
                .HasOne(mr => mr.Advertisement)
                .WithMany(a => a.ModerationReports)
                .HasForeignKey(mr => mr.AdvertisementId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
