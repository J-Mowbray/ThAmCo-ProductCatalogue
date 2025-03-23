// ProductCatalogue/Data/Products/ProductContext.cs
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
            
            // Common configurations for all providers
            p.Property(e => e.Name).IsRequired();
            
            // Create database-specific configurations based on provider
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                // SQL Server specific configurations
                p.Property(e => e.Name).HasMaxLength(250);
                p.Property(e => e.Ean).HasMaxLength(50);
                p.Property(e => e.CategoryName).HasMaxLength(100);
                p.Property(e => e.BrandName).HasMaxLength(100);
                p.Property(e => e.Description).HasColumnType("nvarchar(max)");
                p.Property(e => e.Price).HasColumnType("decimal(18,2)");
                p.Property(e => e.ExpectedRestock).HasColumnType("datetime2");
            }
            
            // Add indexes for all providers
            p.HasIndex(e => e.CategoryId);
            p.HasIndex(e => e.BrandId);
            p.HasIndex(e => e.Ean);
        });
    }
}