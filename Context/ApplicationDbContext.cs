using System.Linq;
using ContentManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ContentManagementSystem.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<AssetItem> AssetItems { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<MaterialItem> MaterialItems { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<MaterialOut> MaterialOuts { get; set; }
        public DbSet<MaterialAssignment> MaterialAssignments { get; set; }

        // Override the Seed method if you want to seed the related tables
        // Configure the model with Fluent API if needed
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Add unique index for InvoiceNo
            modelBuilder.Entity<Material>()
                .HasIndex(m => m.InvoiceNo)
                .IsUnique();

            // Seed Company first
            modelBuilder.Entity<Company>().HasData(
                new Company { Id = 1, Name = "ASP Securities" }
            );

            // Seed Asset Items - only 4 values
            modelBuilder.Entity<AssetItem>().HasData(
                new AssetItem { Id = 1, Name = "Laptop" },
                new AssetItem { Id = 2, Name = "Desktop" },
                new AssetItem { Id = 3, Name = "Server" },
                new AssetItem { Id = 4, Name = "Others" }
            );

            // Seed Vendors - only 4 values
            modelBuilder.Entity<Vendor>().HasData(
                new Vendor { Id = 1, Name = "Dell" },
                new Vendor { Id = 2, Name = "HP" },
                new Vendor { Id = 3, Name = "Lenovo" },
                new Vendor { Id = 4, Name = "Others" }
            );

            // Seed Manufacturers - only 4 values
            modelBuilder.Entity<Manufacturer>().HasData(
                new Manufacturer { Id = 1, Name = "Dell" },
                new Manufacturer { Id = 2, Name = "HP" },
                new Manufacturer { Id = 3, Name = "Lenovo" },
                new Manufacturer { Id = 4, Name = "Others" }
            );

            // Seed Branch data (after Company)
            modelBuilder.Entity<Branch>().HasData(
                new Branch { Id = 1, Name = "Main Branch", CompanyId = 1 },
                new Branch { Id = 2, Name = "North Branch", CompanyId = 1 },
                new Branch { Id = 3, Name = "South Branch", CompanyId = 1 }
            );

            // Configure relationships
            modelBuilder.Entity<Material>()
                .HasOne(m => m.Company)
                .WithMany()
                .HasForeignKey(m => m.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Material>()
                .HasOne(m => m.AssetItem)
                .WithMany()
                .HasForeignKey(m => m.AssetItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Material>()
                .HasOne(m => m.Vendor)
                .WithMany()
                .HasForeignKey(m => m.VendorId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Material>()
                .HasOne(m => m.Manufacturer)
                .WithMany()
                .HasForeignKey(m => m.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<MaterialItem>()
                .HasOne(mi => mi.Material)
                .WithMany(m => m.MaterialItems)
                .HasForeignKey(mi => mi.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Company)
                .WithMany()
                .HasForeignKey(b => b.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaterialOut>()
                .HasOne(m => m.Company)
                .WithMany()
                .HasForeignKey(m => m.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaterialOut>()
                .HasOne(m => m.Branch)
                .WithMany()
                .HasForeignKey(m => m.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaterialItem>()
                .HasOne(mi => mi.AssetItem)
                .WithMany()
                .HasForeignKey(mi => mi.AssetItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaterialAssignment>()
                .HasOne(ma => ma.MaterialOut)
                .WithMany()
                .HasForeignKey(ma => ma.MaterialOutId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaterialAssignment>()
                .HasOne(ma => ma.MaterialItem)
                .WithMany()
                .HasForeignKey(ma => ma.MaterialItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaterialAssignment>()
                .HasOne(ma => ma.Employee)
                .WithMany()
                .HasForeignKey(ma => ma.EmployeeNumber)
                .HasPrincipalKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
            // You can configure additional properties, relationships here if needed
        }
    }
}
