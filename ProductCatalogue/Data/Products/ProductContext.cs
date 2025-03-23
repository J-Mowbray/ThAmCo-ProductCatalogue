// In ProductCatalogue/Data/Products/ProductContext.cs
using Microsoft.EntityFrameworkCore;
using ProductCatalogue.Data.Products;

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
        // Disable auto-generated ID
        p.Property(e => e.Id).ValueGeneratedNever();
        
        // Configure string properties with SQL Server-compatible lengths
        p.Property(e => e.Name).IsRequired().HasMaxLength(250);
        p.Property(e => e.Ean).HasMaxLength(50);
        p.Property(e => e.CategoryName).HasMaxLength(100);
        p.Property(e => e.BrandName).HasMaxLength(100);
        
        // Explicitly set SQL Server types
        p.Property(e => e.Price).HasColumnType("decimal(18,2)");
        p.Property(e => e.ExpectedRestock).HasColumnType("datetime2");
            
        // Add indexes
        p.HasIndex(e => e.CategoryId);
        p.HasIndex(e => e.BrandId);
        p.HasIndex(e => e.Ean);
    });
}
}