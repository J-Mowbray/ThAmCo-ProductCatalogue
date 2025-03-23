using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductCatalogue.Data.Products;
using ProductCatalogue.Services.UnderCutters;

namespace ProductCatalogue.Services;

public class ProductSyncService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ProductSyncService> _logger;

    public ProductSyncService(IServiceProvider services, ILogger<ProductSyncService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Syncing products from supplier");

            try
            {
                // Create a new scope for dependency injection
                using var scope = _services.CreateScope();
                
                // Get the database context and UnderCutters service
                var dbContext = scope.ServiceProvider.GetRequiredService<ProductsContext>();
                var underCutters = scope.ServiceProvider.GetRequiredService<IUnderCuttersService>();
                
                // Get products from UnderCutters
                var supplierProducts = await underCutters.GetProductsAsync();
                _logger.LogInformation("Got {Count} products from supplier", supplierProducts.Count());
                
                // Get existing products from database
                var existingProducts = await dbContext.Products.ToListAsync();
                var existingIds = existingProducts.Select(p => p.Id).ToHashSet();
                
                int added = 0, updated = 0, errors = 0;
                
                // Process each product
                foreach (var dto in supplierProducts)
                {
                    try
                    {
                        if (existingIds.Contains(dto.Id))
                        {
                            // UPDATE existing product
                            var product = existingProducts.First(p => p.Id == dto.Id);
                            var updatedData = MapToDbProduct(dto);
                            
                            // Copy properties from mapped product
                            product.Ean = updatedData.Ean;
                            product.Name = updatedData.Name;
                            product.Description = updatedData.Description;
                            product.CategoryId = updatedData.CategoryId;
                            product.CategoryName = updatedData.CategoryName;
                            product.BrandId = updatedData.BrandId;
                            product.BrandName = updatedData.BrandName;
                            product.Price = updatedData.Price;
                            product.InStock = updatedData.InStock;
                            product.ExpectedRestock = updatedData.ExpectedRestock;
                            
                            updated++;
                        }
                        else
                        {
                            // ADD new product using mapper
                            var newProduct = MapToDbProduct(dto);
                            dbContext.Products.Add(newProduct);
                            added++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing product {Id}", dto.Id);
                        errors++;
                    }
                }
                
                // Save all changes to database
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Product sync completed: Added {Added}, Updated {Updated}, Errors {Errors}", 
                    added, updated, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing products");
            }

            // Wait 24 hours before next sync (as per requirements)
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
    
    private ProductCatalogue.Data.Products.Product MapToDbProduct(ProductDto supplierProduct, decimal markupMultiplier = 1.10m)
    {
        return new ProductCatalogue.Data.Products.Product
        {
            Id = supplierProduct.Id,
            // Clean data to avoid SQL Server errors
            Ean = (supplierProduct.Ean ?? "").Replace(" ", "").Trim(),
            Name = (supplierProduct.Name ?? "Unknown Product").Trim(),
            Description = supplierProduct.Description ?? "",
            CategoryId = supplierProduct.CategoryId,
            CategoryName = (supplierProduct.CategoryName ?? "").Trim(),
            BrandId = supplierProduct.BrandId,
            BrandName = (supplierProduct.BrandName ?? "").Trim(),
            // Apply markup as per requirements
            Price = Math.Round(supplierProduct.Price * markupMultiplier, 2),
            InStock = supplierProduct.InStock,
            // Handle DateTime for SQL Server compatibility
            ExpectedRestock = supplierProduct.ExpectedRestock?.ToUniversalTime()
        };
    }
}