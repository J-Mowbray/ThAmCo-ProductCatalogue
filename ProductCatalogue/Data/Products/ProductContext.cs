using System;
using Microsoft.EntityFrameworkCore;

namespace ProductCatalogue.Data.Products;

public class ProductsContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;

    public ProductsContext(DbContextOptions<ProductsContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>(p =>
        {
            p.Property(c => c.Name).IsRequired();
            p.Property(c => c.Ean).IsRequired(false).HasMaxLength(13);
            p.Property(c => c.CategoryName).IsRequired(false).HasMaxLength(50);
            p.Property(c => c.BrandName).IsRequired(false).HasMaxLength(50);
            p.Property(c => c.Description).IsRequired(false);
            p.Property(c => c.Price).HasPrecision(18, 2);

            p.HasIndex(c => c.CategoryId);
            p.HasIndex(c => c.BrandId);
            
        });
    }
}