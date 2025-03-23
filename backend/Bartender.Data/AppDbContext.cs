using Bartender.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bartender.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Businesses> Businesses { get; set; }
    public DbSet<Places> Places { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<Tables> Tables { get; set; }
    public DbSet<MenuItems> MenuItems { get; set; }
    public DbSet<Products> Products { get; set; }
    public DbSet<Orders> Orders { get; set; }
    public DbSet<ProductsPerOrder> ProductsPerOrders { get; set; }
    public DbSet<Customers> Customers { get; set; }
    public DbSet<Reviews> Reviews { get; set; }
    public DbSet<ProductCategory> ProductCategory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItems>().HasKey(mi => new { mi.PlaceId, mi.ProductId });
        modelBuilder.Entity<ProductsPerOrder>().HasKey(po => new { po.OrderId, po.ProductId });
        modelBuilder.Entity<Reviews>().HasKey(r => new { r.ProductId, r.CustomerId });

        modelBuilder.Entity<Businesses>()
            .Property(b => b.SubscriptionTier)
            .HasConversion<string>();

        modelBuilder.Entity<Staff>()
            .Property(s => s.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Tables>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Orders>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Orders>()
            .Property(o => o.PaymentType)
            .HasConversion<string>();


        base.OnModelCreating(modelBuilder);
    }
}
