﻿using Bartender.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bartender.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Business> Businesses { get; set; }
    public DbSet<Place> Places { get; set; }
    public DbSet<Staff> Staff { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<ProductPerOrder> ProductsPerOrders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ProductCategory> ProductCategory { get; set; }
    public DbSet<GuestSession> GuestSessions { get; set; }
    public DbSet<GuestSessionGroup> GuestSessionGroups { get; set; }
    public DbSet<PlaceImage> PlacePictures { get; set; }
    public DbSet<WeatherData> WeatherDatas { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductPerOrder>().HasKey(po => new { po.OrderId, po.MenuItemId });
        modelBuilder.Entity<Review>().HasKey(r => new { r.ProductId, r.CustomerId });

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Set table names to snake_case
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

            foreach (var property in entity.GetProperties())
            {
                // Set column names to snake_case
                property.SetColumnName(ToSnakeCase(property.GetColumnName()!));
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var builder = new System.Text.StringBuilder();
        builder.Append(char.ToLowerInvariant(input[0]));

        for (int i = 1; i < input.Length; ++i)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                builder.Append('_');
                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }
        }
        return builder.ToString();
    }
}
