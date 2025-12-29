using Microsoft.EntityFrameworkCore;
using bth_dc_inventory.Models;

namespace bth_dc_inventory.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ============================
        // DbSet (Tabel di Database)
        // ============================
        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<DataCenter> DataCenters { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Report> ItemTransaction { get; set; }


        // ============================
        // Fluent API
        // ============================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------------
            // USER
            // -------------------------------
            modelBuilder.Entity<User>()
                .HasMany(u => u.Reports)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------------------------
            // ITEM → CATEGORY
            // -------------------------------
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany()
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------------------------
            // ITEM → DATACENTER
            // -------------------------------
            modelBuilder.Entity<Item>()
                .HasOne(i => i.DataCenter)
                .WithMany()
                .HasForeignKey(i => i.DataCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------------------------
            // ITEM → USER (createdBy)
            // -------------------------------
            modelBuilder.Entity<Item>()
                .HasOne(i => i.CreatedBy)
                .WithMany()
                .HasForeignKey(i => i.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------------------------
            // REPORT → CATEGORY (Optional)
            // -------------------------------
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // -------------------------------
            // REPORT → USER (GeneratedBy)
            // -------------------------------
            modelBuilder.Entity<Report>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------------------------
            // Nama Tabel Biarkan Sesuai Class
            // -------------------------------
        }
    }
}