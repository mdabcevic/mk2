using Microsoft.EntityFrameworkCore;

namespace BartenderBackend.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Business> Businesses { get; set; }
    public DbSet<Places> Places { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<Tables> Tables { get; set; }
    public DbSet<MenuItems> MenuItems { get; set; }
    public DbSet<Products> Products { get; set; }
    public DbSet<Orders> Orders { get; set; }
    public DbSet<ProductsPerOrder> ProductsPerOrders { get; set; }
    public DbSet<Customers> Customers { get; set; }
    public DbSet<Reviews> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItems>().HasKey(mi => new { mi.PlaceId, mi.ProductId });
        modelBuilder.Entity<ProductsPerOrder>().HasKey(po => new { po.OrderId, po.ProductId });
        modelBuilder.Entity<Reviews>().HasKey(r => new { r.ProductId, r.CustomerId });

        base.OnModelCreating(modelBuilder);
    }
}
