using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartInventorySystem.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<StockTransaction> StockTransactions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Seed roles (you already have this)
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = "SuperAdmin", NormalizedName = "SUPERADMIN" },
            new IdentityRole { Id = "2", Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "3", Name = "Staff", NormalizedName = "STAFF" }
        );

        // ── Fix decimal precision warnings ───────────────────────────────────────
        builder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Price).HasPrecision(18, 2);   // or (19,4) etc.
        });

        builder.Entity<Purchase>(entity =>
        {
            entity.Property(e => e.PricePerUnit).HasPrecision(18, 2);
        });

        builder.Entity<Sale>(entity =>
        {
            entity.Property(e => e.PricePerUnit).HasPrecision(18, 2);
        });

        // ── Fix multiple cascade paths ───────────────────────────────────────────
        // Keep cascade: when Product is deleted → delete its Purchases
        builder.Entity<Purchase>()
            .HasOne(p => p.Product)
            .WithMany()                           // no navigation property back, or use collection if you have one
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);    // ← keep this

        // NO cascade: when Supplier is deleted → do NOT auto-delete Purchases
        // (prevents the conflict)
        builder.Entity<Purchase>()
            .HasOne(p => p.Supplier)
            .WithMany()                           // adjust if you have navigation
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);   // ← or DeleteBehavior.NoAction / ClientSetNull

        builder.Entity<AuditLog>()
         .HasOne(a => a.User)
         .WithMany()
         .HasForeignKey(a => a.UserId)
         .OnDelete(DeleteBehavior.SetNull);   // ← this is what you want

        // Optional: same pattern for other entities if needed
        // e.g. Sale → Product (cascade), but Sale has no Supplier FK so no issue there
    }
}